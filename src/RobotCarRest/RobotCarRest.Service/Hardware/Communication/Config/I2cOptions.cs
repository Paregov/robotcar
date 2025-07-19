// Copyright © Svetoslav Paregov. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Paregov.RobotCar.Rest.Service.Hardware.Communication.Config
{
    /// <summary>
    /// Options class for I2C communication configuration.
    /// Used with IOptions pattern for configuration binding.
    /// </summary>
    public class I2cOptions
    {
        /// <summary>
        /// Configuration section name in appsettings.json
        /// </summary>
        public const string SectionName = "Communication:I2c";

        /// <summary>
        /// Gets or sets the I2C bus ID.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "BusId must be 0 or greater")]
        public int BusId { get; set; } = 1;

        /// <summary>
        /// Gets or sets the I2C device address (7-bit or 10-bit).
        /// </summary>
        [Range(0, 1023, ErrorMessage = "DeviceAddress must be between 0 and 1023")]
        public int DeviceAddress { get; set; } = 0x08;

        /// <summary>
        /// Gets or sets whether to use 10-bit addressing.
        /// </summary>
        public bool Use10BitAddressing { get; set; } = false;

        /// <summary>
        /// Gets or sets the bus speed in Hz.
        /// Standard modes: 100kHz (Standard), 400kHz (Fast), 1MHz (Fast+), 3.4MHz (High Speed)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "BusSpeed must be greater than 0")]
        public int BusSpeed { get; set; } = 100_000; // 100 kHz Standard mode

        /// <summary>
        /// Gets or sets the maximum number of bytes to read in a single operation.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "MaxReadLength must be greater than 0")]
        public int MaxReadLength { get; set; } = 256;

        /// <summary>
        /// Gets or sets the maximum number of bytes to write in a single operation.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "MaxWriteLength must be greater than 0")]
        public int MaxWriteLength { get; set; } = 256;

        /// <summary>
        /// Gets or sets whether to use clock stretching.
        /// </summary>
        public bool UseClockStretching { get; set; } = true;

        /// <summary>
        /// Gets or sets the delay between I2C operations in milliseconds.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "OperationDelayMs must be 0 or greater")]
        public int OperationDelayMs { get; set; } = 1;

        /// <summary>
        /// Gets or sets whether to perform address validation before communication.
        /// </summary>
        public bool ValidateAddress { get; set; } = true;

        /// <summary>
        /// Gets or sets the register address byte length (0, 1, or 2 bytes).
        /// 0 = no register addressing, 1 = 8-bit register, 2 = 16-bit register
        /// </summary>
        [Range(0, 2, ErrorMessage = "RegisterAddressLength must be 0, 1, or 2")]
        public int RegisterAddressLength { get; set; } = 1;

        /// <summary>
        /// Gets or sets whether to use repeated start condition for read operations.
        /// </summary>
        public bool UseRepeatedStart { get; set; } = true;

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
        /// Converts the options to an I2cConfig instance.
        /// </summary>
        /// <returns>A configured I2cConfig instance</returns>
        public I2cConfig ToI2cConfig()
        {
            return new I2cConfig
            {
                BusId = BusId,
                DeviceAddress = DeviceAddress,
                Use10BitAddressing = Use10BitAddressing,
                BusSpeed = BusSpeed,
                MaxReadLength = MaxReadLength,
                MaxWriteLength = MaxWriteLength,
                UseClockStretching = UseClockStretching,
                OperationDelayMs = OperationDelayMs,
                ValidateAddress = ValidateAddress,
                RegisterAddressLength = RegisterAddressLength,
                UseRepeatedStart = UseRepeatedStart,
                TimeoutMs = TimeoutMs,
                AutoRetry = AutoRetry,
                MaxRetryAttempts = MaxRetryAttempts,
                RetryDelayMs = RetryDelayMs,
                EnableDebugLogging = EnableDebugLogging
            };
        }
    }
}
