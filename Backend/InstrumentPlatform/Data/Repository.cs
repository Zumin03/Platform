using InstrumentPlatform.Entities;
using Microsoft.EntityFrameworkCore;
using InstrumentPlatform.Enums;
using InstrumentPlatform.Exceptions;
using InstrumentPlatform.Service;

namespace InstrumentPlatform.Data
{
    public class Repository : IRepository
    {
        private readonly ILogger<IRepository> logger;
        private readonly ITimeService timeService;
        private readonly AppDbContext db;

        public Repository(
            ILogger<IRepository> logger,
            AppDbContext db,
            ITimeService timeService)
        {
            this.logger = logger;
            this.db = db;
            this.timeService = timeService;
        }


        /// <inheritdoc/>
        public async Task RegisterInstrument(InstrumentEntity instrument)
        {
            var instrumentExist = await db.Instruments.AnyAsync(i => i.Id == instrument.Id);

            if (!instrumentExist)
            {
                logger.LogInformation($"Adding Instrument with device id: {instrument.Id} to the database.");
                db.Instruments.Add(instrument);
            }
            else
            {
                logger.LogInformation($"Updating fields for device id: {instrument.Id}.");
                db.Instruments.Update(instrument);
            }

            await db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> IsInstrumentAuthorized(string deviceId)
        {
            return await db.Authorized.AnyAsync(i => i.InstrumentId == deviceId);
        }

        /// <inheritdoc/>
        public async Task<InstrumentEntity> GetInstrumentById(string deviceId)
        {
            var instrument =
                await db.Instruments.FirstOrDefaultAsync(i => i.Id == deviceId) ??
                throw new InstrumentNotFoundException(deviceId);

            return instrument;
        }

        /// <inheritdoc/>
        public async Task ResetInstrumentsState()
        {
            logger.LogInformation("Reseting Instruments table to default state");
            await db.Instruments.ExecuteUpdateAsync(setters =>
                    setters
                    .SetProperty(i => i.State, InstrumentState.Disconnected));
        }

        /// <inheritdoc/>
        public async Task<MeasurementEntity> SaveMeasurement(MeasurementEntity measurement)
        {
            try
            {
                logger.LogInformation("Saving measurement to the database...");
                measurement.MeasuredAt = timeService.GenerateUtcTimestamp();
                db.Measurements.Add(measurement);
                await db.SaveChangesAsync();
                logger.LogInformation($"Saved measurement with id: {measurement.Id}");
                return measurement;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MeasurementEntity>> GetMeasurements()
        {
            logger.LogInformation("Getting measurements from the database...");
            try
            {
                IEnumerable<MeasurementEntity> measurements = await db.Measurements.Include(m => m.Instrument).ToListAsync();
                return measurements;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed retrieving measurements from database.\n{ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<InstrumentEntity>> GetInstruments()
        {
            logger.LogInformation("Getting instruments from the database...");
            try
            {
                IEnumerable<InstrumentEntity> instruments = await db.Instruments.ToListAsync();
                return instruments;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed retrieving instruments from database.\n{ex.Message}");
                throw;
            }
        }
    }
}
