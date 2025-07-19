// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Paregov.RobotCar.Rest.Service.Hardware.Communication.Config;
using Paregov.RobotCar.Rest.Service.Hardware.SPI;

namespace Paregov.RobotCar.Rest.Service.Hardware.Communication
{
    /// <summary>
    /// Factory class for creating hardware communication instances with custom configurations.
    /// This factory is now primarily used for creating instances with non-standard configurations
    /// that override the default appsettings.json values.
    /// </summary>
    public class CommunicationFactory
    {
        private readonly ILogger<CommunicationFactory> _logger;
        private readonly IOptions<UartOptions> _uartOptions;
        private readonly IOptions<SpiOptions> _spiOptions;
        private readonly IOptions<I2cOptions> _i2cOptions;

        public CommunicationFactory(
            ILogger<CommunicationFactory> logger,
            IOptions<UartOptions> uartOptions,
            IOptions<SpiOptions> spiOptions,
            IOptions<I2cOptions> i2cOptions)
        {
            _logger = logger;
            _uartOptions = uartOptions;
            _spiOptions = spiOptions;
            _i2cOptions = i2cOptions;
        }

        /// <summary>
        /// Creates and initializes an SPI communication instance with custom configuration.
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="customConfig">Custom SPI configuration (optional - uses appsettings if null)</param>
        /// <returns>Initialized SPI communication instance, or null if initialization failed</returns>
        public SpiCommunication? CreateSpiCommunication(ILogger<SpiCommunication> logger, SpiConfig? customConfig = null)
        {
            try
            {
                // Create instance without auto-initialization by using a temporary options
                var tempOptions = Microsoft.Extensions.Options.Options.Create(_spiOptions.Value);
                var spiComm = new SpiCommunication(logger, tempOptions);
                
                // Free the auto-initialized channel and use custom config if provided
                spiComm.FreeChannel();
                
                var config = customConfig ?? _spiOptions.Value.ToSpiConfig();

                _logger.LogInformation("Creating SPI communication with configuration: {Config}", config.GetConfigurationSummary());

                return spiComm.InitializeChannel(config) ? spiComm : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create SPI communication instance");
                return null;
            }
        }

        /// <summary>
        /// Creates and initializes a UART communication instance with custom configuration.
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="customConfig">Custom UART configuration (optional - uses appsettings if null)</param>
        /// <returns>Initialized UART communication instance, or null if initialization failed</returns>
        public UartCommunication? CreateUartCommunication(ILogger<UartCommunication> logger, UartConfig? customConfig = null)
        {
            try
            {
                // Create instance without auto-initialization by using a temporary options
                var tempOptions = Microsoft.Extensions.Options.Options.Create(_uartOptions.Value);
                var uartComm = new UartCommunication(logger, tempOptions);
                
                // Free the auto-initialized channel and use custom config if provided
                uartComm.FreeChannel();
                
                var config = customConfig ?? _uartOptions.Value.ToUartConfig();

                _logger.LogInformation("Creating UART communication with configuration: {Config}", config.GetConfigurationSummary());

                return uartComm.InitializeChannel(config) ? uartComm : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create UART communication instance");
                return null;
            }
        }

        /// <summary>
        /// Creates and initializes an I2C communication instance with custom configuration.
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="customConfig">Custom I2C configuration (optional - uses appsettings if null)</param>
        /// <param name="deviceAddressOverride">Optional device address override</param>
        /// <returns>Initialized I2C communication instance, or null if initialization failed</returns>
        public I2CCommunication? CreateI2cCommunication(ILogger<I2CCommunication> logger, I2cConfig? customConfig = null, int? deviceAddressOverride = null)
        {
            try
            {
                // Create instance without auto-initialization by using a temporary options
                var tempOptions = Microsoft.Extensions.Options.Options.Create(_i2cOptions.Value);
                var i2cComm = new I2CCommunication(logger, tempOptions);
                
                // Free the auto-initialized channel and use custom config if provided
                i2cComm.FreeChannel();
                
                var config = customConfig ?? _i2cOptions.Value.ToI2cConfig();

                // Override device address if provided
                if (deviceAddressOverride.HasValue)
                {
                    config.DeviceAddress = deviceAddressOverride.Value;
                    _logger.LogInformation("Overriding I2C device address to: 0x{DeviceAddress:X2}", deviceAddressOverride.Value);
                }

                _logger.LogInformation("Creating I2C communication with configuration: {Config}", config.GetConfigurationSummary());

                return i2cComm.InitializeChannel(config) ? i2cComm : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create I2C communication instance");
                return null;
            }
        }

        /// <summary>
        /// Creates a custom SPI configuration for high-speed communication.
        /// This method provides a programmatic override of configuration values.
        /// </summary>
        /// <returns>High-speed SPI configuration</returns>
        public SpiConfig CreateHighSpeedSpiConfig()
        {
            var baseConfig = _spiOptions.Value.ToSpiConfig();

            // Override specific values for high-speed
            baseConfig.ClockFrequency = 1_000_000; // 1 MHz
            baseConfig.MaxMessageLength = 1024;
            baseConfig.OperationDelayMs = 5;
            baseConfig.TimeoutMs = 2000;
            baseConfig.MaxRetryAttempts = 3;
            baseConfig.RetryDelayMs = 50;

            _logger.LogInformation("Created high-speed SPI configuration");
            return baseConfig;
        }

        /// <summary>
        /// Creates a custom UART configuration for high-speed communication.
        /// This method provides a programmatic override of configuration values.
        /// </summary>
        /// <returns>High-speed UART configuration</returns>
        public UartConfig CreateHighSpeedUartConfig()
        {
            var baseConfig = _uartOptions.Value.ToUartConfig();

            // Override specific values for high-speed
            baseConfig.BaudRate = 921600; // High-speed baud rate
            baseConfig.UseFraming = false; // No framing for high-speed
            baseConfig.ReadTimeoutMs = 500;
            baseConfig.WriteTimeoutMs = 500;
            baseConfig.TimeoutMs = 1000;
            baseConfig.MaxRetryAttempts = 2;
            baseConfig.RetryDelayMs = 25;

            _logger.LogInformation("Created high-speed UART configuration");
            return baseConfig;
        }

        /// <summary>
        /// Creates a custom I2C configuration for fast mode communication.
        /// This method provides a programmatic override of configuration values.
        /// </summary>
        /// <param name="deviceAddress">The I2C device address</param>
        /// <returns>Fast mode I2C configuration</returns>
        public I2cConfig CreateFastModeI2cConfig(int deviceAddress)
        {
            var baseConfig = _i2cOptions.Value.ToI2cConfig();

            // Override specific values for fast mode
            baseConfig.DeviceAddress = deviceAddress;
            baseConfig.BusSpeed = 400_000; // 400 kHz Fast mode
            baseConfig.MaxReadLength = 512;
            baseConfig.MaxWriteLength = 512;
            baseConfig.OperationDelayMs = 0; // No delay for fast mode
            baseConfig.ValidateAddress = false; // Skip validation for speed
            baseConfig.TimeoutMs = 500;
            baseConfig.MaxRetryAttempts = 2;
            baseConfig.RetryDelayMs = 10;

            _logger.LogInformation("Created fast mode I2C configuration for device 0x{DeviceAddress:X2}", deviceAddress);
            return baseConfig;
        }
    }
}
