// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using Paregov.RobotCar.Rest.Service.Hardware.Communication.Config;

namespace Paregov.RobotCar.Rest.Service.Hardware.Communication
{
    /// <summary>
    /// Interface for SPI communication operations.
    /// Provides complete SPI communication functionality.
    /// </summary>
    public interface ISpiCommunication : IDisposable
    {
        /// <summary>
        /// Initializes the SPI communication channel with the specified configuration.
        /// </summary>
        /// <param name="config">The SPI configuration instance</param>
        /// <returns>True if initialization was successful; otherwise, false</returns>
        bool InitializeChannel(SpiConfig config);

        /// <summary>
        /// Frees the SPI communication channel and releases associated resources.
        /// </summary>
        /// <returns>True if the channel was successfully freed; otherwise, false</returns>
        bool FreeChannel();

        /// <summary>
        /// Gets whether the SPI communication channel is currently initialized and ready for use.
        /// </summary>
        bool IsChannelReady { get; }

        /// <summary>
        /// Sends a string message through the SPI communication channel.
        /// </summary>
        /// <param name="message">The string message to send</param>
        /// <returns>True if the message was sent successfully; otherwise, false</returns>
        bool SendMessage(string message);

        /// <summary>
        /// Sends a byte array message through the SPI communication channel.
        /// </summary>
        /// <param name="message">The byte array to send</param>
        /// <returns>True if the message was sent successfully; otherwise, false</returns>
        bool SendBytesMessage(byte[] message);

        /// <summary>
        /// Re-initializes the SPI communication channel with a custom clock frequency override.
        /// </summary>
        /// <param name="clockFrequencyOverride">Override clock frequency in Hz</param>
        /// <returns>True if initialization was successful; otherwise, false</returns>
        bool ReinitializeWithClockFrequency(int clockFrequencyOverride);

        /// <summary>
        /// Re-initializes the SPI communication channel with a custom chip select line override.
        /// </summary>
        /// <param name="chipSelectLineOverride">Override chip select line number</param>
        /// <returns>True if initialization was successful; otherwise, false</returns>
        bool ReinitializeWithChipSelectLine(int chipSelectLineOverride);
    }
}
