using InstrumentPlatform.Entities;
using Newtonsoft.Json;
using InstrumentPlatform.Exceptions;
using InstrumentPlatform.Model;
using InstrumentPlatform.Enums;
using InstrumentPlatform.Handlers;
using InstrumentPlatform.Data;

namespace InstrumentPlatform.Service
{
    public class MeasurementService : IMeasurementService
    {
        private readonly IRepository repository;
        private readonly ISerialCommunicationService serialCommunicationService;
        private readonly ILogger<IMeasurementService> logger;
        private readonly IInstrumentErrorHandler instrumentErrorHandler;
        private readonly ITimeService timeService;

        public MeasurementService(
            IRepository repository,
            ISerialCommunicationService serialCommunicationService,
            ILogger<IMeasurementService> logger,
            IInstrumentErrorHandler instrumentErrorHandler,
            ITimeService timeService)
        {
            this.repository = repository;
            this.serialCommunicationService = serialCommunicationService;
            this.logger = logger;
            this.instrumentErrorHandler = instrumentErrorHandler;
            this.timeService = timeService;
        }

        /// <inheritdoc/>
        public async Task<MeasurementDTO> RunMeasurementAsync(string deviceId)
        {
            var instrument = await repository.GetInstrumentById(deviceId);

            try
            {
                logger.LogInformation($"Starting measurement on device {instrument.Id}");
                if (instrument.State.Equals(InstrumentState.Faulted))
                {
                    throw new InstrumentFaultException(instrument.Id);
                }

                var line = serialCommunicationService.SendCommand(InstrumentCommand.Measure, instrument.Port, 9600);

                var result = DeserializeMeasurement(line);
                result = await repository.SaveMeasurement(result);

                return MapMeasurementToDTO(result);
            }
            catch (FileNotFoundException)
            {
                await instrumentErrorHandler.HandleCommunicationError(deviceId);
                throw new InstrumentCommunicationException(deviceId);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MeasurementDTO>> GetMeasurementsAsync()
        {
            var measurements = await repository.GetMeasurements();

            var response = measurements.Select(MapMeasurementToDTO).ToList();
            return response;
        }

        /// <inheritdoc/>
        public MeasurementEntity DeserializeMeasurement(string measurementJSON)
        {
            var result = JsonConvert.DeserializeObject<MeasurementEntity>(measurementJSON)
                ?? throw new DeserializationException(measurementJSON);
            return result;
        }

        private MeasurementDTO MapMeasurementToDTO(MeasurementEntity measurement)
        {
            return new MeasurementDTO(
                measurement.Instrument.Name,
                measurement.Instrument.Id,
                measurement.Instrument.Channel,
                measurement.Value,
                measurement.Instrument.Unit,
                timeService.AdjustTimeToTimezone(measurement.MeasuredAt));
        }
    }
}
