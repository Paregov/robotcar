// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;
using System.IO.Ports;

namespace Paregov.RobotCar.Rest.Service.Hardware.Communication.Config
{
    /// <summary>
    /// Options class for UART communication configuration.
    /// Used with IOptions pattern for configuration binding.
    /// </summary>
    public class UartOptions
    {
        /// <summary>
        /// Configuration section name in appsettings.json
        /// </summary>
        public const string SectionName = "Communication:Uart";

        /// <summary>
        /// Gets or sets the serial port name (e.g., "/dev/serial0", "COM1").
        /// </summary>
        [Required]
        public string PortName { get; set; } = "/dev/serial0";

        /// <summary>
        /// Gets or sets the baud rate for the serial communication.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "BaudRate must be greater than 0")]
        public int BaudRate { get; set; } = 115200;

        /// <summary>
        /// Gets or sets the parity setting for the serial port.
        /// Valid values: "None", "Odd", "Even", "Mark", "Space"
        /// </summary>
        public string Parity { get; set; } = "None";

        /// <summary>
        /// Gets or sets the number of data bits per byte.
        /// </summary>
        [Range(5, 8, ErrorMessage = "DataBits must be between 5 and 8")]
        public int DataBits { get; set; } = 8;

        /// <summary>
        /// Gets or sets the stop bits setting.
        /// Valid values: "None", "One", "Two", "OnePointFive"
        /// </summary>
        public string StopBits { get; set; } = "One";

        /// <summary>
        /// Gets or sets the handshake protocol for the serial port.
        /// Valid values: "None", "XOnXOff", "RequestToSend", "RequestToSendXOnXOff"
        /// </summary>
        public string Handshake { get; set; } = "None";

        /// <summary>
        /// Gets or sets the read timeout in milliseconds.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "ReadTimeoutMs must be greater than 0")]
        public int ReadTimeoutMs { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the write timeout in milliseconds.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "WriteTimeoutMs must be greater than 0")]
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
        /// Gets or sets the timeout for communication operations in milliseconds.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "TimeoutMs must be greater than 0")]
        public int TimeoutMs { get; set; } = 5000;

        /// <summary>
        /// Gets or sets whether the communication channel should be automatically retried on failure.
        /// </summary>
        public bool AutoRetry { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of retry attempts.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "MaxRetryAttempts must be 0 or greater")]
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Gets or sets the delay between retry attempts in milliseconds.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "RetryDelayMs must be 0 or greater")]
        public int RetryDelayMs { get; set; } = 100;

        /// <summary>
        /// Gets or sets whether debug logging is enabled for this communication channel.
        /// </summary>
        public bool EnableDebugLogging { get; set; } = false;

        /// <summary>
        /// Converts the options to a UartConfig instance.
        /// </summary>
        /// <returns>A configured UartConfig instance</returns>
        public UartConfig ToUartConfig()
        {
            return new UartConfig
            {
                PortName = PortName,
                BaudRate = BaudRate,
                Parity = Enum.Parse<Parity>(Parity, true),
                DataBits = DataBits,
                StopBits = Enum.Parse<StopBits>(StopBits, true),
                Handshake = Enum.Parse<Handshake>(Handshake, true),
                ReadTimeoutMs = ReadTimeoutMs,
                WriteTimeoutMs = WriteTimeoutMs,
                UseFraming = UseFraming,
                StartBytes = StartBytes,
                EndBytes = EndBytes,
                TimeoutMs = TimeoutMs,
                AutoRetry = AutoRetry,
                MaxRetryAttempts = MaxRetryAttempts,
                RetryDelayMs = RetryDelayMs,
                EnableDebugLogging = EnableDebugLogging
            };
        }
    }
}
