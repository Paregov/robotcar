// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using System.ComponentModel.DataAnnotations;
using System.Device.Spi;

namespace Paregov.RobotCar.Rest.Service.Hardware.Communication.Config
{
    /// <summary>
    /// Options class for SPI communication configuration.
    /// Used with IOptions pattern for configuration binding.
    /// </summary>
    public class SpiOptions
    {
        /// <summary>
        /// Configuration section name in appsettings.json
        /// </summary>
        public const string SectionName = "Communication:Spi";

        /// <summary>
        /// Gets or sets the SPI bus ID.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "BusId must be 0 or greater")]
        public int BusId { get; set; } = 0;

        /// <summary>
        /// Gets or sets the chip select line number.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "ChipSelectLine must be 0 or greater")]
        public int ChipSelectLine { get; set; } = 0;

        /// <summary>
        /// Gets or sets the clock frequency in Hz.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "ClockFrequency must be greater than 0")]
        public int ClockFrequency { get; set; } = 100_000;

        /// <summary>
        /// Gets or sets the SPI mode (clock polarity and phase).
        /// Valid values: "Mode0", "Mode1", "Mode2", "Mode3"
        /// </summary>
        public string Mode { get; set; } = "Mode0";

        /// <summary>
        /// Gets or sets the data flow direction (MSB or LSB first).
        /// Valid values: "MsbFirst", "LsbFirst"
        /// </summary>
        public string DataFlow { get; set; } = "MsbFirst";

        /// <summary>
        /// Gets or sets whether to use acknowledgment protocol.
        /// </summary>
        public bool UseAcknowledgment { get; set; } = true;

        /// <summary>
        /// Gets or sets the acknowledgment success byte.
        /// </summary>
        [Range(0, 255, ErrorMessage = "AckSuccessByte must be between 0 and 255")]
        public byte AckSuccessByte { get; set; } = 0x06; // ASCII ACK

        /// <summary>
        /// Gets or sets the acknowledgment failure byte.
        /// </summary>
        [Range(0, 255, ErrorMessage = "AckFailureByte must be between 0 and 255")]
        public byte AckFailureByte { get; set; } = 0x15; // ASCII NAK

        /// <summary>
        /// Gets or sets the status request byte.
        /// </summary>
        [Range(0, 255, ErrorMessage = "StatusRequestByte must be between 0 and 255")]
        public byte StatusRequestByte { get; set; } = 0xFF;

        /// <summary>
        /// Gets or sets the maximum message length in bytes.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "MaxMessageLength must be greater than 0")]
        public int MaxMessageLength { get; set; } = 512;

        /// <summary>
        /// Gets or sets the sync bytes for message synchronization.
        /// </summary>
        public byte[] SyncBytes { get; set; } = { 0xAF, 0xAF, 0xAF, 0xAF };

        /// <summary>
        /// Gets or sets the delay between SPI operations in milliseconds.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "OperationDelayMs must be 0 or greater")]
        public int OperationDelayMs { get; set; } = 10;

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
        /// Converts the options to a SpiConfig instance.
        /// </summary>
        /// <returns>A configured SpiConfig instance</returns>
        public SpiConfig ToSpiConfig()
        {
            return new SpiConfig
            {
                BusId = BusId,
                ChipSelectLine = ChipSelectLine,
                ClockFrequency = ClockFrequency,
                Mode = Enum.Parse<SpiMode>(Mode, true),
                DataFlow = Enum.Parse<DataFlow>(DataFlow, true),
                UseAcknowledgment = UseAcknowledgment,
                AckSuccessByte = AckSuccessByte,
                AckFailureByte = AckFailureByte,
                StatusRequestByte = StatusRequestByte,
                MaxMessageLength = MaxMessageLength,
                SyncBytes = SyncBytes,
                OperationDelayMs = OperationDelayMs,
                TimeoutMs = TimeoutMs,
                AutoRetry = AutoRetry,
                MaxRetryAttempts = MaxRetryAttempts,
                RetryDelayMs = RetryDelayMs,
                EnableDebugLogging = EnableDebugLogging
            };
        }
    }
}
