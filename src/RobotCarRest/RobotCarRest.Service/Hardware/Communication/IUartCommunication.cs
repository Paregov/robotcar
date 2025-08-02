// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using Paregov.RobotCar.Rest.Service.Hardware.Communication.Config;

namespace Paregov.RobotCar.Rest.Service.Hardware.Communication
{
    /// <summary>
    /// Interface for UART communication operations.
    /// Provides complete UART communication functionality.
    /// </summary>
    public interface IUartCommunication : IDisposable
    {
        /// <summary>
        /// Initializes the UART communication channel with the specified configuration.
        /// </summary>
        /// <param name="config">The UART configuration instance</param>
        /// <returns>True if initialization was successful; otherwise, false</returns>
        bool InitializeChannel(UartConfig config);

        /// <summary>
        /// Frees the UART communication channel and releases associated resources.
        /// </summary>
        /// <returns>True if the channel was successfully freed; otherwise, false</returns>
        bool FreeChannel();

        /// <summary>
        /// Gets whether the UART communication channel is currently initialized and ready for use.
        /// </summary>
        bool IsChannelReady { get; }

        /// <summary>
        /// Sends a string message through the UART communication channel.
        /// </summary>
        /// <param name="message">The string message to send</param>
        /// <returns>True if the message was sent successfully; otherwise, false</returns>
        bool SendMessage(string message);

        /// <summary>
        /// Sends a byte array message through the UART communication channel.
        /// </summary>
        /// <param name="message">The byte array to send</param>
        /// <returns>True if the message was sent successfully; otherwise, false</returns>
        bool SendBytesMessage(byte[] message);

        /// <summary>
        /// Re-initializes the UART communication channel with a custom port name override.
        /// </summary>
        /// <param name="portNameOverride">Override port name (e.g., "/dev/serial0", "COM1")</param>
        /// <returns>True if initialization was successful; otherwise, false</returns>
        bool ReinitializeWithPortName(string portNameOverride);

        /// <summary>
        /// Re-initializes the UART communication channel with a custom baud rate override.
        /// </summary>
        /// <param name="baudRateOverride">Override baud rate (e.g., 9600, 115200)</param>
        /// <returns>True if initialization was successful; otherwise, false</returns>
        bool ReinitializeWithBaudRate(int baudRateOverride);

        /// <summary>
        /// Closes the UART connection.
        /// </summary>
        void Close();

        /// <summary>
        /// Opens the UART connection.
        /// </summary>
        void Open();

        /// <summary>
        /// Gets whether the UART connection is currently open.
        /// </summary>
        bool IsOpen { get; }
    }
}
