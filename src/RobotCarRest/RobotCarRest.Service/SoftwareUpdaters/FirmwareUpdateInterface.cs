// Copyright © Svetoslav Paregov. All rights reserved.

namespace Paregov.RobotCar.Rest.Service.SoftwareUpdaters
{
    /// <summary>
    /// Defines the communication interface types available for firmware updates.
    /// </summary>
    public enum FirmwareUpdateInterface
    {
        /// <summary>
        /// UART communication interface (default)
        /// </summary>
        Uart = 0,
        
        /// <summary>
        /// SPI communication interface
        /// </summary>
        Spi = 1
    }
}
