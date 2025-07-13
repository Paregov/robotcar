// Copyright © Svetoslav Paregov. All rights reserved.

using Microsoft.Extensions.Logging;
using Paregov.RobotCar.Rest.Service.Hardware.Communication.Configuration;
using Paregov.RobotCar.Rest.Service.Hardware.SPI;

namespace Paregov.RobotCar.Rest.Service.Hardware.Communication
{
    /// <summary>
    /// Factory class for creating hardware communication instances with default configurations.
    /// Provides convenient methods for setting up SPI, UART, and I2C communication.
    /// </summary>
    public static class CommunicationFactory
    {
        /// <summary>
        /// Creates and initializes an SPI communication instance with default configuration.
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <returns>Initialized SPI communication instance, or null if initialization failed</returns>
        public static SpiCommunication? CreateSpiCommunication(ILogger<SpiCommunication> logger)
        {
            var spiComm = new SpiCommunication(logger);
            var config = new SpiConfig
            {
                BusId = 0,
                ChipSelectLine = 0,
                ClockFrequency = 100_000,
                Mode = System.Device.Spi.SpiMode.Mode0,
                UseAcknowledgment = true,
                MaxMessageLength = 512,
                OperationDelayMs = 10,
                EnableDebugLogging = false
            };

            return spiComm.InitializeChannel(config) ? spiComm : null;
        }

        /// <summary>
        /// Creates and initializes a UART communication instance with default configuration.
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <returns>Initialized UART communication instance, or null if initialization failed</returns>
        public static UartCommunication? CreateUartCommunication(ILogger<UartCommunication> logger)
        {
            var uartComm = new UartCommunication(logger);
            var config = new UartConfig
            {
                PortName = "/dev/serial0",
                BaudRate = 115200,
                Parity = System.IO.Ports.Parity.None,
                DataBits = 8,
                StopBits = System.IO.Ports.StopBits.One,
                UseFraming = true,
                StartBytes = new byte[] { 0xAA, 0xBB, 0xCC },
                EndBytes = new byte[] { 0xDD, 0xEE, 0xFF },
                ReadTimeoutMs = 1000,
                WriteTimeoutMs = 1000,
                EnableDebugLogging = false
            };

            return uartComm.InitializeChannel(config) ? uartComm : null;
        }

        /// <summary>
        /// Creates and initializes an I2C communication instance with default configuration.
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="deviceAddress">The I2C device address (default: 0x08)</param>
        /// <returns>Initialized I2C communication instance, or null if initialization failed</returns>
        public static I2cCommunication? CreateI2cCommunication(ILogger<I2cCommunication> logger, int deviceAddress = 0x08)
        {
            var i2cComm = new I2cCommunication(logger);
            var config = new I2cConfig
            {
                BusId = 1,
                DeviceAddress = deviceAddress,
                Use10BitAddressing = false,
                BusSpeed = 100_000, // 100 kHz Standard mode
                MaxReadLength = 256,
                MaxWriteLength = 256,
                UseClockStretching = true,
                OperationDelayMs = 1,
                ValidateAddress = true,
                RegisterAddressLength = 1,
                UseRepeatedStart = true,
                EnableDebugLogging = false
            };

            return i2cComm.InitializeChannel(config) ? i2cComm : null;
        }

        /// <summary>
        /// Creates a custom SPI configuration for high-speed communication.
        /// </summary>
        /// <returns>High-speed SPI configuration</returns>
        public static SpiConfig CreateHighSpeedSpiConfig()
        {
            return new SpiConfig
            {
                BusId = 0,
                ChipSelectLine = 0,
                ClockFrequency = 1_000_000, // 1 MHz
                Mode = System.Device.Spi.SpiMode.Mode0,
                UseAcknowledgment = true,
                MaxMessageLength = 1024,
                OperationDelayMs = 5,
                EnableDebugLogging = false,
                TimeoutMs = 2000,
                AutoRetry = true,
                MaxRetryAttempts = 3,
                RetryDelayMs = 50
            };
        }

        /// <summary>
        /// Creates a custom UART configuration for high-speed communication.
        /// </summary>
        /// <returns>High-speed UART configuration</returns>
        public static UartConfig CreateHighSpeedUartConfig()
        {
            return new UartConfig
            {
                PortName = "/dev/serial0",
                BaudRate = 921600, // High-speed baud rate
                Parity = System.IO.Ports.Parity.None,
                DataBits = 8,
                StopBits = System.IO.Ports.StopBits.One,
                Handshake = System.IO.Ports.Handshake.None,
                UseFraming = false, // No framing for high-speed
                ReadTimeoutMs = 500,
                WriteTimeoutMs = 500,
                EnableDebugLogging = false,
                TimeoutMs = 1000,
                AutoRetry = true,
                MaxRetryAttempts = 2,
                RetryDelayMs = 25
            };
        }

        /// <summary>
        /// Creates a custom I2C configuration for fast mode communication.
        /// </summary>
        /// <param name="deviceAddress">The I2C device address</param>
        /// <returns>Fast mode I2C configuration</returns>
        public static I2cConfig CreateFastModeI2cConfig(int deviceAddress)
        {
            return new I2cConfig
            {
                BusId = 1,
                DeviceAddress = deviceAddress,
                Use10BitAddressing = false,
                BusSpeed = 400_000, // 400 kHz Fast mode
                MaxReadLength = 512,
                MaxWriteLength = 512,
                UseClockStretching = true,
                OperationDelayMs = 0, // No delay for fast mode
                ValidateAddress = false, // Skip validation for speed
                RegisterAddressLength = 1,
                UseRepeatedStart = true,
                EnableDebugLogging = false,
                TimeoutMs = 500,
                AutoRetry = true,
                MaxRetryAttempts = 2,
                RetryDelayMs = 10
            };
        }
    }
}
