// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using System.IO.Ports;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Paregov.RobotCar.Rest.Service.Hardware.Communication.Config;

namespace Paregov.RobotCar.Rest.Service.Hardware.Communication
{
    /// <summary>
    /// Manages UART communication using SerialPort for sending strings and bytes
    /// with proper configuration and error handling.
    /// </summary>
    public class UartCommunication : IUartCommunication
    {
        private readonly ILogger<UartCommunication> _logger;
        private readonly IOptions<UartOptions> _options;
        private readonly object _lock = new();
        
        private SerialPort? _serialPort;
        private UartConfig? _config;

        /// <summary>
        /// Initializes a new instance of the UartCommunication class.
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="options">UART configuration options</param>
        public UartCommunication(
            ILogger<UartCommunication> logger,
            IOptions<UartOptions> options)
        {
            _logger = logger;
            _options = options;
            
            // Auto-initialize with the provided configuration
            var config = _options.Value.ToUartConfig();
            if (!InitializeChannel(config))
            {
                _logger.LogWarning("Failed to auto-initialize UART communication channel during construction");
            }
        }

        /// <summary>
        /// Gets whether the communication channel is currently initialized and ready for use.
        /// </summary>
        public bool IsChannelReady => _serialPort?.IsOpen ?? false;

        /// <summary>
        /// Initializes the UART communication channel with the specified configuration.
        /// </summary>
        /// <param name="config">The UART configuration instance</param>
        /// <returns>True if initialization was successful; otherwise, false</returns>
        public bool InitializeChannel(UartConfig config)
        {
            if (config == null)
            {
                _logger.LogError("UART configuration cannot be null.");
                return false;
            }

            if (!config.IsValid())
            {
                _logger.LogError("Invalid UART configuration provided.");
                return false;
            }

            try
            {
                // Free existing channel if present
                FreeChannel();

                _config = config;

                _serialPort = new SerialPort(
                    _config.PortName,
                    _config.BaudRate,
                    _config.Parity,
                    _config.DataBits,
                    _config.StopBits)
                {
                    Handshake = _config.Handshake,
                    ReadTimeout = _config.ReadTimeoutMs,
                    WriteTimeout = _config.WriteTimeoutMs
                };

                _serialPort.Open();
                
                _logger.LogInformation("UART device initialized successfully. {Config}", _config.GetConfigurationSummary());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize the UART serial port: {Message}", ex.Message);
                FreeChannel();
                return false;
            }
        }

        /// <summary>
        /// Re-initializes the UART communication channel with a custom port name override.
        /// </summary>
        /// <param name="portNameOverride">Override port name</param>
        /// <returns>True if initialization was successful; otherwise, false</returns>
        public bool ReinitializeWithPortName(string portNameOverride)
        {
            _logger.LogInformation("Re-initializing UART with port name override: {PortName}", portNameOverride);
            
            var config = _options.Value.ToUartConfig();
            config.PortName = portNameOverride;
            
            return InitializeChannel(config);
        }

        /// <summary>
        /// Re-initializes the UART communication channel with a custom baud rate override.
        /// </summary>
        /// <param name="baudRateOverride">Override baud rate</param>
        /// <returns>True if initialization was successful; otherwise, false</returns>
        public bool ReinitializeWithBaudRate(int baudRateOverride)
        {
            _logger.LogInformation("Re-initializing UART with baud rate override: {BaudRate}", baudRateOverride);
            
            var config = _options.Value.ToUartConfig();
            config.BaudRate = baudRateOverride;
            
            return InitializeChannel(config);
        }

        /// <summary>
        /// Frees the UART communication channel and releases associated resources.
        /// </summary>
        /// <returns>True if the channel was successfully freed; otherwise, false</returns>
        public bool FreeChannel()
        {
            try
            {
                _serialPort?.Close();
                _serialPort?.Dispose();
                _serialPort = null;
                _config = null;
                _logger.LogInformation("UART communication channel freed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error freeing UART communication channel: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Sends a string message over UART with protocol framing.
        /// </summary>
        /// <param name="message">The string message to send</param>
        /// <returns>True if the message was sent successfully; otherwise, false</returns>
        public bool SendMessage(string message)
        {
            if (!IsChannelReady)
            {
                _logger.LogWarning("Cannot send message. UART serial port is not initialized or open.");
                return false;
            }

            if (string.IsNullOrEmpty(message))
            {
                _logger.LogWarning("Cannot send an empty message.");
                return false;
            }

            try
            {
                lock (_lock)
                {
                    string messageToSend = _config!.UseFraming 
                        ? BuildFramedCommand(message) 
                        : message;

                    if (_config.EnableDebugLogging)
                    {
                        _logger.LogDebug("Sending UART message: {Message}", messageToSend);
                    }

                    _serialPort!.Write(messageToSend);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during UART message transmission: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Sends raw bytes over UART.
        /// </summary>
        /// <param name="message">The byte array to send</param>
        /// <returns>True if the bytes were sent successfully; otherwise, false</returns>
        public bool SendBytesMessage(byte[] message)
        {
            if (!IsChannelReady)
            {
                _logger.LogWarning("Cannot send bytes. UART serial port is not initialized or open.");
                return false;
            }

            if (message == null || message.Length == 0)
            {
                _logger.LogWarning("Cannot send empty byte array.");
                return false;
            }

            try
            {
                lock (_lock)
                {
                    if (_config!.EnableDebugLogging)
                    {
                        _logger.LogDebug("Sending UART bytes: [{Bytes}]", string.Join(", ", message));
                    }

                    _serialPort!.Write(message, 0, message.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during UART bytes transmission: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Closes the UART connection.
        /// </summary>
        public void Close()
        {
            try
            {
                _serialPort?.Close();
                _logger.LogInformation("UART serial port closed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing UART serial port: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Opens the UART connection.
        /// </summary>
        public void Open()
        {
            try
            {
                if (_serialPort != null && !_serialPort.IsOpen)
                {
                    _serialPort.Open();
                    _logger.LogInformation("UART serial port opened.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening UART serial port: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Gets whether the UART connection is open.
        /// </summary>
        public bool IsOpen => _serialPort?.IsOpen ?? false;

        /// <summary>
        /// Builds a framed command with start and end bytes.
        /// </summary>
        /// <param name="command">The command string to frame</param>
        /// <returns>The framed command string</returns>
        private string BuildFramedCommand(string command)
        {
            if (_config?.UseFraming != true)
                return command;

            var stringBuilder = new StringBuilder();
            
            // Convert byte arrays to chars for framing
            foreach (byte b in _config.StartBytes)
                stringBuilder.Append((char)b);
            
            stringBuilder.Append(command);
            
            foreach (byte b in _config.EndBytes)
                stringBuilder.Append((char)b);

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Cleans up the UART resources.
        /// </summary>
        public void Dispose()
        {
            FreeChannel();
        }
    }
}
