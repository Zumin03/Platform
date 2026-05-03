using InstrumentPlatform.Entities;

namespace InstrumentPlatform.Data
{
    public interface IRepository
    {

        /// <summary>
        /// Retrieves an instrument by its device identifier.
        /// </summary>
        /// <param name="deviceId">The unique device identifier of the instrument.</param>
        /// <returns>Returns a <see cref="InstrumentEntity"/>.</returns>
        Task<InstrumentEntity> GetInstrumentById(string deviceId);

        /// <summary>
        /// Retrieves all measurement record from the database.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<InstrumentEntity>> GetInstruments();

        /// <summary>
        /// Retrieves all measurement record from the database.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MeasurementEntity>> GetMeasurements();

        /// <summary>
        /// Checks whether the instrument is authorized in the database.
        /// </summary>
        /// <param name="deviceId">The unique device identifier of the instrument.</param>
        /// <returns>Returns a <see cref="bool"/> indicating whether the instrument is authorized.</returns>
        Task<bool> IsInstrumentAuthorized(string deviceId);

        /// <summary>
        /// Registers a new Instrument in the database.
        /// </summary>
        /// <param name="instrument">The instrument to register.</param>
        Task RegisterInstrument(InstrumentEntity instrument);

        /// <summary>
        /// Resets the instruments connected and operational fields to default.
        /// </summary>
        /// <returns>Returns a <see cref="Task"/>.</returns>
        Task ResetInstrumentsState();

        /// <summary>
        /// Saves a measurement record to the database.
        /// </summary>
        /// <param name="measurement">The measurement entity to save.</param>
        /// <returns>Returns a <see cref="MeasurementEntity"/>.</returns>
        Task<MeasurementEntity> SaveMeasurement(MeasurementEntity measurement);
    }
}
