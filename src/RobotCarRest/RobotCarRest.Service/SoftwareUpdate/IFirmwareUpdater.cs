// Copyright © Svetoslav Paregov. All rights reserved.

namespace Paregov.RobotCar.Rest.Service.SoftwareUpdate
{
    public interface IFirmwareUpdater
    {
        public (bool, string) UpdateLowLevelController(
            byte[]? firmwareData,
            FirmwareUpdateInterface updateInterface = FirmwareUpdateInterface.Uart);
    }
}
