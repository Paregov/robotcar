﻿// Copyright © Svetoslav Paregov. All rights reserved.

using System.Device.Spi;

namespace Paregov.RobotCar.Rest.Service.Hardware.Communication.Config
{
    /// <summary>
    /// Configuration class for SPI communication parameters.
    /// Contains all necessary settings for SPI bus communication.
    /// </summary>
    public class SpiConfig : CommunicationConfigBase
    {
        /// <summary>
        /// Gets or sets the SPI bus ID.
        /// </summary>
        public int BusId { get; set; } = 0;

        /// <summary>
        /// Gets or sets the chip select line number.
        /// </summary>
        public int ChipSelectLine { get; set; } = 0;

        /// <summary>
        /// Gets or sets the clock frequency in Hz.
        /// </summary>
        public int ClockFrequency { get; set; } = 100_000;

        /// <summary>
        /// Gets or sets the SPI mode (clock polarity and phase).
        /// </summary>
        public SpiMode Mode { get; set; } = SpiMode.Mode0;

        /// <summary>
        /// Gets or sets the data flow direction (MSB or LSB first).
        /// </summary>
        public DataFlow DataFlow { get; set; } = DataFlow.MsbFirst;

        /// <summary>
        /// Gets or sets whether to use acknowledgment protocol.
        /// </summary>
        public bool UseAcknowledgment { get; set; } = true;

        /// <summary>
        /// Gets or sets the acknowledgment success byte.
        /// </summary>
        public byte AckSuccessByte { get; set; } = 0x06; // ASCII ACK

        /// <summary>
        /// Gets or sets the acknowledgment failure byte.
        /// </summary>
        public byte AckFailureByte { get; set; } = 0x15; // ASCII NAK

        /// <summary>
        /// Gets or sets the status request byte.
        /// </summary>
        public byte StatusRequestByte { get; set; } = 0xFF;

        /// <summary>
        /// Gets or sets the maximum message length in bytes.
        /// </summary>
        public int MaxMessageLength { get; set; } = 512;

        /// <summary>
        /// Gets or sets the sync bytes for message synchronization.
        /// </summary>
        public byte[] SyncBytes { get; set; } = { 0xAF, 0xAF, 0xAF, 0xAF };

        /// <summary>
        /// Gets or sets the delay between SPI operations in milliseconds.
        /// </summary>
        public int OperationDelayMs { get; set; } = 10;

        /// <summary>
        /// Validates the SPI configuration parameters.
        /// </summary>
        /// <returns>True if the configuration is valid; otherwise, false.</returns>
        public override bool IsValid()
        {
            return base.IsValid() &&
                   BusId >= 0 &&
                   ChipSelectLine >= 0 &&
                   ClockFrequency > 0 &&
                   MaxMessageLength > 0 &&
                   OperationDelayMs >= 0 &&
                   SyncBytes?.Length > 0;
        }

        /// <summary>
        /// Gets a detailed string representation of the SPI configuration.
        /// </summary>
        /// <returns>A formatted string containing the SPI configuration details.</returns>
        public override string GetConfigurationSummary()
        {
            var baseConfig = base.GetConfigurationSummary();
            return $"SPI Config - Bus: {BusId}, CS: {ChipSelectLine}, Frequency: {ClockFrequency}Hz, Mode: {Mode}, DataFlow: {DataFlow}, UseAck: {UseAcknowledgment}, MaxLength: {MaxMessageLength}, OpDelay: {OperationDelayMs}ms, {baseConfig}";
        }
    }
}
