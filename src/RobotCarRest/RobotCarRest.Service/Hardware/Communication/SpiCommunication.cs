// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using System.Device.Spi;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Paregov.RobotCar.Rest.Service.Hardware.Communication;
using Paregov.RobotCar.Rest.Service.Hardware.Communication.Config;

namespace Paregov.RobotCar.Rest.Service.Hardware.SPI
{
    /// <summary>
    /// Manages SPI communication on a Raspberry Pi for sending strings
    /// with a built-in confirmation mechanism.
    /// This class acts as the SPI Master.
    /// </summary>
    public class SpiCommunication : IHardwareCommunication
    {
        private readonly ILogger<SpiCommunication> _logger;
        private readonly IOptions<SpiOptions> _options;
        private SpiDevice? _spiDevice;
        private SpiConfig? _config;

        /// <summary>
        /// Initializes a new instance of the SpiCommunication class.
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="options">SPI configuration options</param>
        public SpiCommunication(
            ILogger<SpiCommunication> logger,
            IOptions<SpiOptions> options)
        {
            _logger = logger;
            _options = options;
            
            // Auto-initialize with the provided configuration
            var config = _options.Value.ToSpiConfig();
            if (!InitializeChannel(config))
            {
                _logger.LogWarning("Failed to auto-initialize SPI communication channel during construction");
            }
        }

        /// <summary>
        /// Gets whether the communication channel is currently initialized and ready for use.
        /// </summary>
        public bool IsChannelReady => _spiDevice != null && _config != null;

        /// <summary>
        /// Initializes the SPI communication channel with the specified configuration.
        /// </summary>
        /// <param name="config">The SPI configuration instance</param>
        /// <returns>True if initialization was successful; otherwise, false</returns>
        public bool InitializeChannel(CommunicationConfigBase config)
        {
            if (config is not SpiConfig spiConfig)
            {
                _logger.LogError("Invalid configuration type. Expected SpiConfig.");
                return false;
            }

            if (!spiConfig.IsValid())
            {
                _logger.LogError("Invalid SPI configuration provided.");
                return false;
            }

            try
            {
                // Dispose existing device if present
                FreeChannel();

                _config = spiConfig;

                SpiConnectionSettings connectionSettings = new(_config.BusId, _config.ChipSelectLine)
                {
                    ClockFrequency = _config.ClockFrequency,
                    Mode = _config.Mode,
                    DataFlow = _config.DataFlow
                };

                _spiDevice = SpiDevice.Create(connectionSettings);
                
                _logger.LogInformation($"SPI device initialized successfully. {_config.GetConfigurationSummary()}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing SPI device: {Message}", ex.Message);
                _logger.LogError("Please ensure SPI is enabled on your Raspberry Pi ('sudo raspi-config') and the application is run with sufficient permissions ('sudo').");
                FreeChannel();
                return false;
            }
        }

        /// <summary>
        /// Re-initializes the SPI communication channel with a custom clock frequency override.
        /// </summary>
        /// <param name="clockFrequencyOverride">Override clock frequency</param>
        /// <returns>True if initialization was successful; otherwise, false</returns>
        public bool ReinitializeWithClockFrequency(int clockFrequencyOverride)
        {
            _logger.LogInformation("Re-initializing SPI with clock frequency override: {ClockFrequency} Hz", clockFrequencyOverride);
            
            var config = _options.Value.ToSpiConfig();
            config.ClockFrequency = clockFrequencyOverride;
            
            return InitializeChannel(config);
        }

        /// <summary>
        /// Re-initializes the SPI communication channel with a custom chip select line override.
        /// </summary>
        /// <param name="chipSelectLineOverride">Override chip select line</param>
        /// <returns>True if initialization was successful; otherwise, false</returns>
        public bool ReinitializeWithChipSelectLine(int chipSelectLineOverride)
        {
            _logger.LogInformation("Re-initializing SPI with chip select line override: {ChipSelectLine}", chipSelectLineOverride);
            
            var config = _options.Value.ToSpiConfig();
            config.ChipSelectLine = chipSelectLineOverride;
            
            return InitializeChannel(config);
        }

        /// <summary>
        /// Frees the SPI communication channel and releases associated resources.
        /// </summary>
        /// <returns>True if the channel was successfully freed; otherwise, false</returns>
        public bool FreeChannel()
        {
            try
            {
                _spiDevice?.Dispose();
                _spiDevice = null;
                _config = null;
                _logger.LogInformation("SPI communication channel freed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error freeing SPI communication channel: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Encodes and sends a string message over SPI and waits for a confirmation.
        /// </summary>
        /// <param name="message">The string message to send</param>
        /// <returns>True if the slave device acknowledged successful receipt; otherwise, false</returns>
        public bool SendMessage(string message)
        {
            if (!IsChannelReady)
            {
                _logger.LogWarning("Cannot send message. SPI device is not initialized.");
                return false;
            }

            if (string.IsNullOrEmpty(message))
            {
                _logger.LogWarning("Cannot send an empty message.");
                return false;
            }

            // Convert the string to a byte array
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            uint messageLength = (uint)messageBytes.Length;

            if (messageLength > _config!.MaxMessageLength)
            {
                _logger.LogError($"Error: Message is too long. Maximum size is {_config.MaxMessageLength} bytes, but message is {messageLength} bytes.");
                return false;
            }

            byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
            
            try
            {
                if (_config.EnableDebugLogging)
                {
                    _logger.LogDebug($"Sending sync bytes: [{string.Join(", ", _config.SyncBytes)}]...");
                }
                SendByteArray(_config.SyncBytes);

                if (_config.EnableDebugLogging)
                {
                    _logger.LogDebug($"Sending message length {messageLength} in {messageLengthBytes.Length} bytes [{string.Join(", ", messageLengthBytes)}]...");
                }
                SendByteArray(messageLengthBytes);

                if (_config.EnableDebugLogging)
                {
                    _logger.LogDebug($"Sending {messageBytes.Length} bytes [{string.Join(", ", messageBytes)}]...");
                }
                SendByteArray(messageBytes);
                SendByteArray(new byte[] { 0x00 }); // Send a dummy byte to signal the end of the message

                if (_config.UseAcknowledgment)
                {
                    return WaitForAcknowledgment();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during SPI transfer: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Sends raw bytes over SPI.
        /// </summary>
        /// <param name="message">The byte array to send</param>
        /// <returns>True if the bytes were sent successfully; otherwise, false</returns>
        public bool SendBytesMessage(byte[] message)
        {
            if (!IsChannelReady)
            {
                _logger.LogWarning("Cannot send bytes. SPI device is not initialized.");
                return false;
            }

            if (message == null || message.Length == 0)
            {
                _logger.LogWarning("Cannot send empty byte array.");
                return false;
            }

            try
            {
                SendByteArray(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bytes over SPI: {Message}", ex.Message);
                return false;
            }
        }

        private bool WaitForAcknowledgment()
        {
            try
            {
                // Dummy byte to send
                byte[] statusRequestBuffer = { _config!.StatusRequestByte };
                // Buffer to receive the ACK/NAK
                byte[] statusResponseBuffer = new byte[1];

                _spiDevice!.TransferFullDuplex(statusRequestBuffer, statusResponseBuffer);

                byte confirmationByte = statusResponseBuffer[0];

                if (_config.EnableDebugLogging)
                {
                    _logger.LogDebug($"Received confirmation byte: 0x{confirmationByte:X2}");
                }

                if (confirmationByte == _config.AckSuccessByte)
                {
                    _logger.LogInformation("Success: Received ACK from slave.");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"Failure: Received NAK (or unexpected byte: 0x{confirmationByte:X2}) from slave.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error waiting for acknowledgment: {Message}", ex.Message);
                return false;
            }
        }

        private void SendByteArray(byte[] data)
        {
            if (!IsChannelReady)
            {
                _logger.LogError("SPI device is not initialized. Cannot send data.");
                return;
            }

            try
            {
                foreach (byte b in data)
                {
                    _spiDevice!.WriteByte(b);
                }

                if (_config!.OperationDelayMs > 0)
                {
                    Thread.Sleep(_config.OperationDelayMs);
                }
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
            FreeChannel();
        }
    }
}
