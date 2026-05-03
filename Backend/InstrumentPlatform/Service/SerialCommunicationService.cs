namespace InstrumentPlatform.Service
{
    using System.IO.Ports;
    using InstrumentPlatform.Enums;
    using InstrumentPlatform.Extensions;

    public class SerialCommunicationService : ISerialCommunicationService
    {
        private readonly ILogger<ISerialCommunicationService> logger;

        /// <inheritdoc/>
        public SerialCommunicationService(
            ILogger<ISerialCommunicationService> logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public string SendCommand(InstrumentCommand command, string port, int baudRate, int timeOut = 5000)
        {
            logger.LogInformation($"Opening serial port \"{port}\" with baud rate: \"{baudRate}\"");
            using var serial = new SerialPort(port, baudRate);
            serial.Encoding = System.Text.Encoding.UTF8;
            serial.ReadTimeout = timeOut;
            serial.Open();

            logger.LogInformation($"Executing command: \"{command}\" on serial port: \"{port}\"");
            serial.WriteLine(command.ToCommandString());
            var response = serial.ReadLine().Trim();
            serial.Close();

            return response;
        }
    }
}
