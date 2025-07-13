// Copyright © Svetoslav Paregov. All rights reserved.

namespace Paregov.RobotCar.Rest.Service.SoftwareUpdaters
{
    public interface IFirmwareUpdater
    {
        public bool UpdateLowLevelController(byte[]? firmwareData);
        
        public bool UpdateLowLevelController(byte[]? firmwareData, FirmwareUpdateInterface updateInterface);
    }
}
