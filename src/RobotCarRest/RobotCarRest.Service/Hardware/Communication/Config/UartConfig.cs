// Copyright © Svetoslav Paregov. All rights reserved.

using System.IO.Ports;

namespace Paregov.RobotCar.Rest.Service.Hardware.Communication.Configuration
{
    /// <summary>
    /// Configuration class for UART communication parameters.
    /// Contains all necessary settings for serial port communication.
    /// </summary>
    public class UartConfig : CommunicationConfigBase
    {
        /// <summary>
        /// Gets or sets the serial port name (e.g., "/dev/serial0", "COM1").
        /// </summary>
        public string PortName { get; set; } = "/dev/serial0";

        /// <summary>
        /// Gets or sets the baud rate for the serial communication.
        /// </summary>
        public int BaudRate { get; set; } = 115200;

        /// <summary>
        /// Gets or sets the parity setting for the serial port.
        /// </summary>
        public Parity Parity { get; set; } = Parity.None;

        /// <summary>
        /// Gets or sets the number of data bits per byte.
        /// </summary>
        public int DataBits { get; set; } = 8;

        /// <summary>
        /// Gets or sets the stop bits setting.
        /// </summary>
        public StopBits StopBits { get; set; } = StopBits.One;

        /// <summary>
        /// Gets or sets the handshake protocol for the serial port.
        /// </summary>
        public Handshake Handshake { get; set; } = Handshake.None;

        /// <summary>
        /// Gets or sets the read timeout in milliseconds.
        /// </summary>
        public int ReadTimeoutMs { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the write timeout in milliseconds.
        /// </summary>
        public int WriteTimeoutMs { get; set; } = 1000;

        /// <summary>
        /// Gets or sets whether to use message framing with start/end bytes.
        /// </summary>
        public bool UseFraming { get; set; } = true;

        /// <summary>
        /// Gets or sets the start bytes for message framing.
        /// </summary>
        public byte[] StartBytes { get; set; } = { 0xAA, 0xBB, 0xCC };

        /// <summary>
        /// Gets or sets the end bytes for message framing.
        /// </summary>
        public byte[] EndBytes { get; set; } = { 0xDD, 0xEE, 0xFF };

        /// <summary>
        /// Validates the UART configuration parameters.
        /// </summary>
        /// <returns>True if the configuration is valid; otherwise, false.</returns>
        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(PortName) &&
                   BaudRate > 0 &&
                   DataBits >= 5 && DataBits <= 8 &&
                   ReadTimeoutMs > 0 &&
                   WriteTimeoutMs > 0 &&
                   (!UseFraming || StartBytes?.Length > 0 && EndBytes?.Length > 0);
        }

        /// <summary>
        /// Gets a detailed string representation of the UART configuration.
        /// </summary>
        /// <returns>A formatted string containing the UART configuration details.</returns>
        public override string GetConfigurationSummary()
        {
            var baseConfig = base.GetConfigurationSummary();
            return $"UART Config - Port: {PortName}, Baud: {BaudRate}, Parity: {Parity}, DataBits: {DataBits}, StopBits: {StopBits}, Handshake: {Handshake}, ReadTimeout: {ReadTimeoutMs}ms, WriteTimeout: {WriteTimeoutMs}ms, UseFraming: {UseFraming}, {baseConfig}";
        }
    }
}
