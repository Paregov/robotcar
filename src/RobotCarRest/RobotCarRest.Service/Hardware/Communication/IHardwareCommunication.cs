// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using Paregov.RobotCar.Rest.Service.Hardware.Communication.Config;

namespace Paregov.RobotCar.Rest.Service.Hardware.Communication
{
    public interface IHardwareCommunication : IDisposable
    {
        /// <summary>
        /// Initializes the communication channel with the specified configuration.
        /// </summary>
        /// <param name="config">The configuration instance for the specific communication protocol</param>
        /// <returns>True if initialization was successful; otherwise, false</returns>
        bool InitializeChannel(CommunicationConfigBase config);

        /// <summary>
        /// Frees the communication channel and releases associated resources.
        /// </summary>
        /// <returns>True if the channel was successfully freed; otherwise, false</returns>
        bool FreeChannel();

        /// <summary>
        /// Gets whether the communication channel is currently initialized and ready for use.
        /// </summary>
        bool IsChannelReady { get; }

        /// <summary>
        /// Sends a string message through the communication channel.
        /// </summary>
        /// <param name="message">The string message to send</param>
        /// <returns>True if the message was sent successfully; otherwise, false</returns>
        bool SendMessage(string message);

        /// <summary>
        /// Sends a byte array message through the communication channel.
        /// </summary>
        /// <param name="message">The byte array to send</param>
        /// <returns>True if the message was sent successfully; otherwise, false</returns>
        bool SendBytesMessage(byte[] message);
    }
}
