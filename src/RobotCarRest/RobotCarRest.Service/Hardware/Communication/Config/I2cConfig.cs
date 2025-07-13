// Copyright © Svetoslav Paregov. All rights reserved.


// Copyright © Svetoslav Paregov. All rights reserved.

namespace Paregov.RobotCar.Rest.Service.Hardware.Communication.Configuration
{
    /// <summary>
    /// Configuration class for I2C communication parameters.
    /// Contains all necessary settings for I2C bus communication.
    /// </summary>
    public class I2cConfig : CommunicationConfigBase
    {
        /// <summary>
        /// Gets or sets the I2C bus ID.
        /// </summary>
        public int BusId { get; set; } = 1;

        /// <summary>
        /// Gets or sets the I2C device address (7-bit or 10-bit).
        /// </summary>
        public int DeviceAddress { get; set; } = 0x08;

        /// <summary>
        /// Gets or sets whether to use 10-bit addressing.
        /// </summary>
        public bool Use10BitAddressing { get; set; } = false;

        /// <summary>
        /// Gets or sets the bus speed in Hz.
        /// Standard modes: 100kHz (Standard), 400kHz (Fast), 1MHz (Fast+), 3.4MHz (High Speed)
        /// </summary>
        public int BusSpeed { get; set; } = 100_000; // 100 kHz Standard mode

        /// <summary>
        /// Gets or sets the maximum number of bytes to read in a single operation.
        /// </summary>
        public int MaxReadLength { get; set; } = 256;

        /// <summary>
        /// Gets or sets the maximum number of bytes to write in a single operation.
        /// </summary>
        public int MaxWriteLength { get; set; } = 256;

        /// <summary>
        /// Gets or sets whether to use clock stretching.
        /// </summary>
        public bool UseClockStretching { get; set; } = true;

        /// <summary>
        /// Gets or sets the delay between I2C operations in milliseconds.
        /// </summary>
        public int OperationDelayMs { get; set; } = 1;

        /// <summary>
        /// Gets or sets whether to perform address validation before communication.
        /// </summary>
        public bool ValidateAddress { get; set; } = true;

        /// <summary>
        /// Gets or sets the register address byte length (0, 1, or 2 bytes).
        /// 0 = no register addressing, 1 = 8-bit register, 2 = 16-bit register
        /// </summary>
        public int RegisterAddressLength { get; set; } = 1;

        /// <summary>
        /// Gets or sets whether to use repeated start condition for read operations.
        /// </summary>
        public bool UseRepeatedStart { get; set; } = true;

        /// <summary>
        /// Validates the I2C configuration parameters.
        /// </summary>
        /// <returns>True if the configuration is valid; otherwise, false.</returns>
        public override bool IsValid()
        {
            return base.IsValid() &&
                   BusId >= 0 &&
                   DeviceAddress >= 0 &&
                   (Use10BitAddressing ? DeviceAddress <= 0x3FF : DeviceAddress <= 0x7F) &&
                   BusSpeed > 0 &&
                   MaxReadLength > 0 &&
                   MaxWriteLength > 0 &&
                   OperationDelayMs >= 0 &&
                   RegisterAddressLength >= 0 && RegisterAddressLength <= 2;
        }

        /// <summary>
        /// Gets a detailed string representation of the I2C configuration.
        /// </summary>
        /// <returns>A formatted string containing the I2C configuration details.</returns>
        public override string GetConfigurationSummary()
        {
            var baseConfig = base.GetConfigurationSummary();
            return $"I2C Config - Bus: {BusId}, Address: 0x{DeviceAddress:X2}, 10-bit: {Use10BitAddressing}, Speed: {BusSpeed}Hz, MaxRead: {MaxReadLength}, MaxWrite: {MaxWriteLength}, ClockStretch: {UseClockStretching}, RegAddrLen: {RegisterAddressLength}, RepeatStart: {UseRepeatedStart}, OpDelay: {OperationDelayMs}ms, {baseConfig}";
        }
    }
}
