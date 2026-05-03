using FluentAssertions;
using InstrumentPlatform.Data;
using InstrumentPlatform.Entities;
using InstrumentPlatform.Enums;
using InstrumentPlatform.Exceptions;
using InstrumentPlatform.Handlers;
using InstrumentPlatform.Service;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;

namespace IntrumentPlatform.Test
{
    public class MeasurementServiceTest
    {

        [Fact]
        public void DeserializeMeasurement_ValidJson_ReturnsMeasurementEntity()
        {
            // Arrange
            var service = new MeasurementService(
                Substitute.For<IRepository>(),
                Substitute.For<ISerialCommunicationService>(),
                Substitute.For<ILogger<MeasurementService>>(),
                Substitute.For<IInstrumentErrorHandler>(),
                Substitute.For<ITimeService>());

            var measurementJSON = @"{
            ""instrumentId"": ""123"",
            ""value"": 32.5
            }";

            // Act
            var result = service.DeserializeMeasurement(measurementJSON);

            // Assert
            result.Should().NotBeNull();
            result.InstrumentId.Should().Be("123");
            result.Value.Should().Be(32.5f);
        }

        [Fact]
        public void DeserializeMeasurement_InvalidJson_ThrowsDeserializationException()
        {
            // Arrange
            var service = new MeasurementService(
                Substitute.For<IRepository>(),
                Substitute.For<ISerialCommunicationService>(),
                Substitute.For<ILogger<MeasurementService>>(),
                Substitute.For<IInstrumentErrorHandler>(),
                Substitute.For<ITimeService>());

            var invalidJson = @"{ ""Id"": ""123"", ""Value"": ""NaN"" ";

            // Act & Assert
            Assert.Throws<JsonSerializationException>(() =>
                service.DeserializeMeasurement(invalidJson)
            );
        }

        [Fact]
        public async Task GetMeasurementsAsync_ValidData_ReturnsMappedDTOs()
        {
            // Arrange
            var repositoryMock = Substitute.For<IRepository>();

            var instrument = new InstrumentEntity(
                    id: "test0",
                    name: "test_instrument",
                    channel: "temperature",
                    unit: "K",
                    softwareVersion: "1.0",
                    port: "COM0",
                    state: InstrumentState.Connected);

            IEnumerable<MeasurementEntity> measurement = [new MeasurementEntity(
                id: 123,
                instrumentId: "test0",
                value: 10.5f,
                measuredAt: DateTime.UtcNow,
                instrument: instrument)];

            repositoryMock.GetMeasurements().Returns(measurement);
            var service = new MeasurementService(
                repositoryMock,
                Substitute.For<ISerialCommunicationService>(),
                Substitute.For<ILogger<MeasurementService>>(),
                Substitute.For<IInstrumentErrorHandler>(),
                Substitute.For<ITimeService>());

            // Act
            var results = await service.GetMeasurementsAsync();

            // Assert
            await repositoryMock.Received(1).GetMeasurements();

            results.Should().NotBeNull();
            results.Should().HaveCount(1);
            var result = results.Single();

            result.DeviceName.Should().Be("test_instrument");
            result.DeviceId.Should().Be("test0");
            result.Value.Should().Be(10.5f);
            result.Unit.Should().Be("K");
            result.Channel.Should().Be("temperature");
        }

        [Fact]
        public async Task RunMeasurementAsync_ThrowsInstrumentCommunicationException()
        {
            // Arrange
            var repositoryMock = Substitute.For<IRepository>();
            var serialServiceMock = Substitute.For<ISerialCommunicationService>();
            var instruermentErrorHandlerMock = Substitute.For<IInstrumentErrorHandler>();
            var deviceId = "testId";

            var instrument = new InstrumentEntity(
                    id: deviceId,
                    name: "test_instrument",
                    channel: "temperature",
                    unit: "K",
                    softwareVersion: "1.0",
                    port: "COM0",
                    state: InstrumentState.Connected);

            repositoryMock.GetInstrumentById(deviceId).Returns(instrument);

            serialServiceMock.SendCommand(InstrumentCommand.Measure, "COM0", 9600)
                .Returns(x =>
                    {
                        throw new FileNotFoundException();
                    });

            var service = new MeasurementService(
                repositoryMock,
                serialServiceMock,
                Substitute.For<ILogger<MeasurementService>>(),
                instruermentErrorHandlerMock,
                Substitute.For<ITimeService>());

            // Act + Assert
            await Assert.ThrowsAsync<InstrumentCommunicationException>(() => service.RunMeasurementAsync(deviceId));

            await instruermentErrorHandlerMock.Received(1).HandleCommunicationError(deviceId);
        }
    }
}
