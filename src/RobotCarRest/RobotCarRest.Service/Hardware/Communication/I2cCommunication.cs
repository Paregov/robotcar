// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using System.Device.I2c;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Paregov.RobotCar.Rest.Service.Hardware.Communication.Configuration;

namespace Paregov.RobotCar.Rest.Service.Hardware.Communication
{
    /// <summary>
    /// Manages I2C communication for sending strings and bytes
    /// with proper configuration and error handling.
    /// </summary>
    public class I2cCommunication : IHardwareCommunication
    {
        private readonly ILogger<I2cCommunication> _logger;
        private readonly object _lock = new();
        
        private I2cDevice? _i2cDevice;
        private I2cConfig? _config;

        /// <summary>
        /// Initializes a new instance of the I2cCommunication class.
        /// </summary>
        /// <param name="logger">Logger instance</param>
        public I2cCommunication(ILogger<I2cCommunication> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets whether the communication channel is currently initialized and ready for use.
        /// </summary>
        public bool IsChannelReady => _i2cDevice != null && _config != null;

        /// <summary>
        /// Initializes the I2C communication channel with the specified configuration.
        /// </summary>
        /// <param name="config">The I2C configuration instance</param>
        /// <returns>True if initialization was successful; otherwise, false</returns>
        public bool InitializeChannel(CommunicationConfigBase config)
        {
            if (config is not I2cConfig i2cConfig)
            {
                _logger.LogError("Invalid configuration type. Expected I2cConfig.");
                return false;
            }

            if (!i2cConfig.IsValid())
            {
                _logger.LogError("Invalid I2C configuration provided.");
                return false;
            }

            try
            {
                // Free existing channel if present
                FreeChannel();

                _config = i2cConfig;

                I2cConnectionSettings connectionSettings = new(_config.BusId, _config.DeviceAddress);

                _i2cDevice = I2cDevice.Create(connectionSettings);

                // Validate device address if requested
                if (_config.ValidateAddress)
                {
                    ValidateDeviceAddress();
                }

                _logger.LogInformation($"I2C device initialized successfully. {_config.GetConfigurationSummary()}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize the I2C device: {Message}", ex.Message);
                FreeChannel();
                return false;
            }
        }

        /// <summary>
        /// Frees the I2C communication channel and releases associated resources.
        /// </summary>
        /// <returns>True if the channel was successfully freed; otherwise, false</returns>
        public bool FreeChannel()
        {
            try
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null;
                _config = null;
                _logger.LogInformation("I2C communication channel freed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error freeing I2C communication channel: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Sends a string message over I2C.
        /// </summary>
        /// <param name="message">The string message to send</param>
        /// <returns>True if the message was sent successfully; otherwise, false</returns>
        public bool SendMessage(string message)
        {
            if (!IsChannelReady)
            {
                _logger.LogWarning("Cannot send message. I2C device is not initialized.");
                return false;
            }

            if (string.IsNullOrEmpty(message))
            {
                _logger.LogWarning("Cannot send an empty message.");
                return false;
            }

            try
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                
                if (messageBytes.Length > _config!.MaxWriteLength)
                {
                    _logger.LogError($"Message too long. Maximum length is {_config.MaxWriteLength} bytes, but message is {messageBytes.Length} bytes.");
                    return false;
                }

                lock (_lock)
                {
                    if (_config.EnableDebugLogging)
                    {
                        _logger.LogDebug("Sending I2C message: {Message}", message);
                    }

                    _i2cDevice!.Write(messageBytes);

                    if (_config.OperationDelayMs > 0)
                    {
                        Thread.Sleep(_config.OperationDelayMs);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during I2C message transmission: {Message}", ex.Message);
                return TryWithRetry(() => SendMessage(message));
            }
        }

        /// <summary>
        /// Sends raw bytes over I2C.
        /// </summary>
        /// <param name="message">The byte array to send</param>
        /// <returns>True if the bytes were sent successfully; otherwise, false</returns>
        public bool SendBytesMessage(byte[] message)
        {
            if (!IsChannelReady)
            {
                _logger.LogWarning("Cannot send bytes. I2C device is not initialized.");
                return false;
            }

            if (message == null || message.Length == 0)
            {
                _logger.LogWarning("Cannot send empty byte array.");
                return false;
            }

            if (message.Length > _config!.MaxWriteLength)
            {
                _logger.LogError($"Message too long. Maximum length is {_config.MaxWriteLength} bytes, but message is {message.Length} bytes.");
                return false;
            }

            try
            {
                lock (_lock)
                {
                    if (_config.EnableDebugLogging)
                    {
                        _logger.LogDebug($"Sending I2C bytes: [{string.Join(", ", message)}]");
                    }

                    _i2cDevice!.Write(message);

                    if (_config.OperationDelayMs > 0)
                    {
                        Thread.Sleep(_config.OperationDelayMs);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during I2C bytes transmission: {Message}", ex.Message);
                return TryWithRetry(() => SendBytesMessage(message));
            }
        }

        /// <summary>
        /// Writes data to a specific register on the I2C device.
        /// </summary>
        /// <param name="registerAddress">The register address to write to</param>
        /// <param name="data">The data to write</param>
        /// <returns>True if the write was successful; otherwise, false</returns>
        public bool WriteToRegister(int registerAddress, byte[] data)
        {
            if (!IsChannelReady)
            {
                _logger.LogWarning("Cannot write to register. I2C device is not initialized.");
                return false;
            }

            if (data == null || data.Length == 0)
            {
                _logger.LogWarning("Cannot write empty data to register.");
                return false;
            }

            try
            {
                lock (_lock)
                {
                    byte[] writeBuffer = new byte[_config!.RegisterAddressLength + data.Length];
                    
                    // Add register address bytes
                    if (_config.RegisterAddressLength == 1)
                    {
                        writeBuffer[0] = (byte)registerAddress;
                    }
                    else if (_config.RegisterAddressLength == 2)
                    {
                        writeBuffer[0] = (byte)(registerAddress >> 8);
                        writeBuffer[1] = (byte)(registerAddress & 0xFF);
                    }

                    // Add data
                    Array.Copy(data, 0, writeBuffer, _config.RegisterAddressLength, data.Length);

                    if (_config.EnableDebugLogging)
                    {
                        _logger.LogDebug($"Writing to I2C register 0x{registerAddress:X2}: [{string.Join(", ", data)}]");
                    }

                    _i2cDevice!.Write(writeBuffer);

                    if (_config.OperationDelayMs > 0)
                    {
                        Thread.Sleep(_config.OperationDelayMs);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing to I2C register 0x{RegisterAddress:X2}: {Message}", registerAddress, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Reads data from a specific register on the I2C device.
        /// </summary>
        /// <param name="registerAddress">The register address to read from</param>
        /// <param name="length">The number of bytes to read</param>
        /// <returns>The read data, or null if the operation failed</returns>
        public byte[]? ReadFromRegister(int registerAddress, int length)
        {
            if (!IsChannelReady)
            {
                _logger.LogWarning("Cannot read from register. I2C device is not initialized.");
                return null;
            }

            if (length <= 0 || length > _config!.MaxReadLength)
            {
                _logger.LogWarning($"Invalid read length. Must be between 1 and {_config.MaxReadLength}.");
                return null;
            }

            try
            {
                lock (_lock)
                {
                    byte[] registerAddressBytes = new byte[_config.RegisterAddressLength];
                    
                    if (_config.RegisterAddressLength == 1)
                    {
                        registerAddressBytes[0] = (byte)registerAddress;
                    }
                    else if (_config.RegisterAddressLength == 2)
                    {
                        registerAddressBytes[0] = (byte)(registerAddress >> 8);
                        registerAddressBytes[1] = (byte)(registerAddress & 0xFF);
                    }

                    byte[] readBuffer = new byte[length];

                    if (_config.UseRepeatedStart)
                    {
                        _i2cDevice!.WriteRead(registerAddressBytes, readBuffer);
                    }
                    else
                    {
                        _i2cDevice!.Write(registerAddressBytes);
                        Thread.Sleep(_config.OperationDelayMs);
                        _i2cDevice.Read(readBuffer);
                    }

                    if (_config.EnableDebugLogging)
                    {
                        _logger.LogDebug($"Read from I2C register 0x{registerAddress:X2}: [{string.Join(", ", readBuffer)}]");
                    }

                    return readBuffer;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading from I2C register 0x{RegisterAddress:X2}: {Message}", registerAddress, ex.Message);
                return null;
            }
        }

        private void ValidateDeviceAddress()
        {
            try
            {
                // Try to read a single byte to validate the device address
                byte[] testBuffer = new byte[1];
                _i2cDevice!.Read(testBuffer);
                _logger.LogInformation($"I2C device address 0x{_config!.DeviceAddress:X2} validated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"I2C device address 0x{_config!.DeviceAddress:X2} validation failed: {ex.Message}");
                throw;
            }
        }

        private bool TryWithRetry(Func<bool> operation)
        {
            if (!_config?.AutoRetry ?? false)
                return false;

            for (int attempt = 1; attempt <= _config.MaxRetryAttempts; attempt++)
            {
                try
                {
                    Thread.Sleep(_config.RetryDelayMs);
                    
                    if (operation())
                    {
                        _logger.LogInformation($"I2C operation succeeded on retry attempt {attempt}.");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"I2C retry attempt {attempt} failed: {ex.Message}");
                }
            }

            _logger.LogError($"I2C operation failed after {_config.MaxRetryAttempts} retry attempts.");
            return false;
        }

        /// <summary>
        /// Cleans up the I2C resources.
        /// </summary>
        public void Dispose()
        {
            FreeChannel();
        }
    }
}
