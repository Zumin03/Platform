using FluentAssertions;
using InstrumentPlatform.Data;
using InstrumentPlatform.Entities;
using InstrumentPlatform.Enums;
using InstrumentPlatform.Exceptions;
using InstrumentPlatform.Handlers;
using InstrumentPlatform.Service;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace IntrumentPlatform.Test
{
    public class InstrumentServiceTest
    {
        [Fact]
        public void DeserializeInstrument_ValidJson_ReturnsInstrumentEntity()
        {
            // Arrange
            var service = new InstrumentService(
                Substitute.For<IRepository>(),
                Substitute.For<ISerialCommunicationService>(),
                Substitute.For<ILogger<InstrumentService>>(),
                Substitute.For<IInstrumentErrorHandler>());

            var json = @"{
                ""instrumentId"": ""dev1"",
                ""instrumentName"": ""TestDevice"",
                ""channel"": ""temperature"",
                ""unit"": ""K"",
                ""softwareVersion"": ""1.0"",
                ""port"": ""COM1"",
            }";

            // Act
            var result = service.DeserializeInstrument(json);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("dev1");
            result.Name.Should().Be("TestDevice");
            result.Channel.Should().Be("temperature");
            result.Unit.Should().Be("K");
            result.SoftwareVersion.Should().Be("1.0");
            result.Port.Should().Be("COM1");
            result.State.Should().Be(InstrumentState.Connected);
        }

        [Fact]
        public void DeserializeInstrument_InvalidJson_ThrowsInvalidOperationException()
        {
            // Arrange
            var service = new InstrumentService(
                Substitute.For<IRepository>(),
                Substitute.For<ISerialCommunicationService>(),
                Substitute.For<ILogger<InstrumentService>>(),
                Substitute.For<IInstrumentErrorHandler>());

            var json = "null";

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() => service.DeserializeInstrument(json));
        }

        [Fact]
        public void DeserializeSelfTest_ValidJson_ReturnsSelfTestDTO()
        {
            // Arrange
            var service = new InstrumentService(
                Substitute.For<IRepository>(),
                Substitute.For<ISerialCommunicationService>(),
                Substitute.For<ILogger<InstrumentService>>(),
                Substitute.For<IInstrumentErrorHandler>());

            var json = @"{
                ""deviceId"": ""dev1"",
                ""sensorState"": ""OK"",
            }";

            // Act
            var result = service.DeserializeSelfTest(json);

            // Assert
            result.Should().NotBeNull();
            result.DeviceId.Should().Be("dev1");
            result.SensorState.Should().Be("OK");
        }

        [Fact]
        public void DeserializeSelfTest_InvalidJson_ThrowsInvalidOperationException()
        {
            // Arrange
            var service = new InstrumentService(
                Substitute.For<IRepository>(),
                Substitute.For<ISerialCommunicationService>(),
                Substitute.For<ILogger<InstrumentService>>(),
                Substitute.For<IInstrumentErrorHandler>());

            var json = "null";

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() => service.DeserializeSelfTest(json));
        }

        [Fact]
        public void GetSelfTestResult_ValidJson_ReturnsInstrumentState()
        {
            var service = new InstrumentService(
                Substitute.For<IRepository>(),
                Substitute.For<ISerialCommunicationService>(),
                Substitute.For<ILogger<InstrumentService>>(),
                Substitute.For<IInstrumentErrorHandler>());

            var json = @"{
                ""deviceId"": ""dev1"",
                ""sensorState"": ""OK"",
            }";

            // Act
            var result = service.GetSelfTestResult(json);

            // Assert
            result.Should().Be(InstrumentState.Connected);
        }

        [Fact]
        public async Task GetInstrumentsAsync_RetunsMappedDTOs()
        {
            // Arrange
            var repositoryMock = Substitute.For<IRepository>();

            var instruments = new List<InstrumentEntity>
            {
                new InstrumentEntity(
                    id: "dev1",
                    name: "Device 1",
                    channel: "temperature",
                    unit: "K",
                    softwareVersion: "1.0",
                    port: "COM1",
                    state: InstrumentState.Connected),

                new InstrumentEntity(
                    id: "dev2",
                    name: "Device 2",
                    channel: "pressure",
                    unit: "Pa",
                    softwareVersion: "1.1",
                    port: "COM2",
                    state: InstrumentState.Faulted)
            };

            repositoryMock.GetInstruments().Returns(instruments);

            var service = new InstrumentService(
                repositoryMock,
                Substitute.For<ISerialCommunicationService>(),
                Substitute.For<ILogger<InstrumentService>>(),
                Substitute.For<IInstrumentErrorHandler>());

            // Act
            var result = (await service.GetInstrumentsAsync()).ToList();

            // Assert
            result.Should().HaveCount(2);

            result[0].DeviceId.Should().Be("dev1");
            result[0].DeviceName.Should().Be("Device 1");
            result[0].Channel.Should().Be("temperature");
            result[0].SoftwareVersion.Should().Be("1.0");
            result[0].InstrumentState.Should().Be("Connected");

            result[1].DeviceId.Should().Be("dev2");
            result[1].DeviceName.Should().Be("Device 2");
            result[1].Channel.Should().Be("pressure");
            result[1].SoftwareVersion.Should().Be("1.1");
            result[1].InstrumentState.Should().Be("Faulted");

            await repositoryMock.Received(1).GetInstruments();
        }

        [Fact]
        public async Task GetInstrumentAsync_RetunsMappedDTOs()
        {
            // Arrange
            var repositoryMock = Substitute.For<IRepository>();

            var instrument = new InstrumentEntity(
                id: "dev1",
                name: "Device 1",
                channel: "temperature",
                unit: "K",
                softwareVersion: "1.0",
                port: "COM1",
                state: InstrumentState.Connected);


            repositoryMock.GetInstrumentById(Arg.Any<string>()).Returns(instrument);

            var service = new InstrumentService(
                repositoryMock,
                Substitute.For<ISerialCommunicationService>(),
                Substitute.For<ILogger<InstrumentService>>(),
                Substitute.For<IInstrumentErrorHandler>());

            // Act
            var result = await service.GetInstrumentAsync("dev1");

            // Assert
            result.Should().NotBeNull();

            result.DeviceId.Should().Be("dev1");
            result.DeviceName.Should().Be("Device 1");
            result.Channel.Should().Be("temperature");
            result.SoftwareVersion.Should().Be("1.0");
            result.InstrumentState.Should().Be("Connected");

            await repositoryMock.Received(1).GetInstrumentById("dev1");
        }

        [Fact]
        public async Task RunSelfTest_ReturnsInstrumentDTO()
        {
            // Arrange
            var repositoryMock = Substitute.For<IRepository>();
            var serialCommunicationServiceMock = Substitute.For<ISerialCommunicationService>();

            var instrument = new InstrumentEntity(
                id: "dev1",
                name: "Device 1",
                channel: "temperature",
                unit: "K",
                softwareVersion: "1.0",
                port: "COM1",
                state: InstrumentState.Connected);

            repositoryMock.GetInstrumentById(Arg.Any<string>()).Returns(instrument);
            serialCommunicationServiceMock
                .SendCommand(
                    Arg.Any<InstrumentCommand>(),
                    Arg.Any<string>(),
                    Arg.Any<int>())
                .Returns(@"{
                    ""deviceId"": ""dev1"",
                    ""sensorState"": ""OK"",
                }");

            var service = new InstrumentService(
                repositoryMock,
                serialCommunicationServiceMock,
                Substitute.For<ILogger<InstrumentService>>(),
                Substitute.For<IInstrumentErrorHandler>());

            // Act
            var result = await service.RunSelfTest("dev1");

            // Assert
            result.Should().NotBeNull();

            result.DeviceId.Should().Be("dev1");
            result.DeviceName.Should().Be("Device 1");
            result.Channel.Should().Be("temperature");
            result.SoftwareVersion.Should().Be("1.0");
            result.InstrumentState.Should().Be("Connected");

            await repositoryMock.Received(1).GetInstrumentById("dev1");
            await repositoryMock.Received(1).RegisterInstrument(instrument);
            serialCommunicationServiceMock.Received(1).SendCommand(InstrumentCommand.SelfTest, "COM1", 9600);
        }

        [Fact]
        public async Task RunSelfTest_WhenFileNotFoundThrown_ThrowsInstrumentCommunicationException()
        {
            // Arrange
            var repositoryMock = Substitute.For<IRepository>();
            var serialCommunicationServiceMock = Substitute.For<ISerialCommunicationService>();

            var instrument = new InstrumentEntity(
                id: "dev1",
                name: "Device 1",
                channel: "temperature",
                unit: "K",
                softwareVersion: "1.0",
                port: "COM1",
                state: InstrumentState.Connected);

            repositoryMock.GetInstrumentById(Arg.Any<string>()).Returns(instrument);
            serialCommunicationServiceMock
                .SendCommand(
                    Arg.Any<InstrumentCommand>(),
                    Arg.Any<string>(),
                    Arg.Any<int>())
                .Throws(new FileNotFoundException());

            var service = new InstrumentService(
                repositoryMock,
                serialCommunicationServiceMock,
                Substitute.For<ILogger<InstrumentService>>(),
                Substitute.For<IInstrumentErrorHandler>());

            // Act + Assert
            var exception = await Assert.ThrowsAsync<InstrumentCommunicationException>(() =>
                service.RunSelfTest("dev1"));

            exception.DeviceId.Should().Be("dev1");
        }
    }
}
