using InstrumentPlatform.Data;
using InstrumentPlatform.Enums;

namespace InstrumentPlatform.Handlers
{
    /// <summary>
    /// Provides functionality for handling instrument-related errors.
    /// </summary>
    public class InstrumentErrorHandler : IInstrumentErrorHandler
    {
        private readonly IRepository repository;

        public InstrumentErrorHandler(IRepository repository)
        {
            this.repository = repository;
        }

        /// <inheritdoc/>
        public async Task<string> HandleCommunicationError(string deviceId)
        {
            var instrument = await repository.GetInstrumentById(deviceId);
            instrument.State = InstrumentState.Faulted;
            await repository.RegisterInstrument(instrument);

            return instrument.Id;
        }
    }
}
