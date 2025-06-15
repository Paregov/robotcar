// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using System.Device.Spi;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace NetworkController.Hardware
{
    /// <summary>
    /// Manages SPI communication on a Raspberry Pi for sending strings
    /// with a built-in confirmation mechanism.
    /// This class acts as the SPI Master.
    /// </summary>
    public class SpiCommunicator : ISpiCommunicator
    {
        private readonly ILogger<SpiCommunicator> _logger;

        // Define protocol constants for acknowledgment
        private const byte ACK_SUCCESS = 0x06; // Standard ASCII Acknowledge
        private const byte NAK_FAILURE = 0x15; // Standard ASCII Negative Acknowledge

        // The byte sent by the master to clock the line and request the ACK/NAK from the slave.
        private const byte REQUEST_STATUS_BYTE = 0xFF;

        private SpiDevice? _spiDevice;

        /// <summary>
        /// Initializes a new instance of the SpiCommunicator class.
        /// </summary>
        /// <param name="logger">Logger</param>
        public SpiCommunicator(
            ILogger<SpiCommunicator> logger)
        {
            _logger = logger;
            int busId = 0;
            int chipSelectLine = 0;
            int clockFrequency = 100_000;

            SpiConnectionSettings connectionSettings = new(busId, chipSelectLine)
            {
                ClockFrequency = clockFrequency,
                Mode = SpiMode.Mode0
            };

            try
            {
                _spiDevice = SpiDevice.Create(connectionSettings);
                _logger.LogInformation($"SPI device initialized on bus {busId}, CS {chipSelectLine} at {clockFrequency} Hz.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error initializing SPI device: {ex.Message}");
               _logger.LogError("Please ensure SPI is enabled on your Raspberry Pi ('sudo raspi-config') and the application is run with sufficient permissions ('sudo').");
                //throw;
            }
        }

        /// <summary>
        /// Encodes and sends a string message over SPI and waits for a confirmation.
        /// </summary>
        /// <param name="message">The string message to send. Max length is 255 characters.</param>
        /// <returns>True if the slave device acknowledged successful receipt; otherwise, false.</returns>
        public bool SendMessage(string message)
        {
            if (_spiDevice == null)
            {
                _logger.LogInformation("Cannot send message. SPI device is not initialized.");
                return false;
            }

            if (string.IsNullOrEmpty(message))
            {
                _logger.LogInformation("Cannot send an empty message.");
                return false;
            }

            // Convert the string to a byte array
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);

            UInt32 messageLength = (UInt32)messageBytes.Length;

            if (messageLength > 512)
            {
                _logger.LogError($"Error: Message is too long. Maximum size is 512 bytes, but message is {messageLength} bytes.");

                return false;
            }

            byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
            
            try
            {
                byte[] syncBytes = { 0xAF, 0xAF, 0xAF, 0xAF };
                _logger.LogInformation($"Sending sync bytes: [{string.Join(", ", syncBytes)}]...");
                SendByteArray(syncBytes);

                _logger.LogInformation($"Sending message length {messageLength} in {messageLengthBytes.Length} bytes [{string.Join(", ", messageLengthBytes)}]...");
                SendByteArray(messageLengthBytes);

                _logger.LogInformation($"Sending {messageBytes.Length} bytes [{string.Join(", ", messageBytes)}]...");
                SendByteArray(messageBytes);
                SendByteArray(new byte[] { 0x00 }); // Send a dummy byte to signal the end of the message

                // Dummy byte to send
                byte[] statusRequestBuffer = { REQUEST_STATUS_BYTE };
                // Buffer to receive the ACK/NAK
                byte[] statusResponseBuffer = new byte[1];

                _spiDevice.TransferFullDuplex(statusRequestBuffer, statusResponseBuffer);

                byte confirmationByte = statusResponseBuffer[0];

                _logger.LogInformation($"Received confirmation byte: 0x{confirmationByte:X2}");

                if (confirmationByte == ACK_SUCCESS)
                {
                    _logger.LogInformation("Success: Received ACK from slave.");
                    return true;
                }
                else
                {
                    _logger.LogInformation($"Failure: Received NAK (or unexpected byte: 0x{confirmationByte:X2}) from slave.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"An error occurred during SPI transfer: {ex.Message}");
                return false;
            }
        }

        public bool SendBytesMessage(byte[] message)
        {
            SendByteArray(message);

            return true;
        }

        private void SendByteArray(byte[] data)
        {
            if (_spiDevice == null)
            {
                _logger.LogError("SPI device is not initialized. Cannot send data.");
                return;
            }
            try
            {
                foreach (byte b in data)
                {
                    _spiDevice.WriteByte(b);
                }

                Thread.Sleep(10); // Optional delay to ensure data is sent properly
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending data over SPI: {ex.Message}");
            }
        }

        /// <summary>
        /// Cleans up the SPI device resources.
        /// </summary>
        public void Dispose()
        {
            _spiDevice?.Dispose();
            _spiDevice = null;
        }
    }
}
