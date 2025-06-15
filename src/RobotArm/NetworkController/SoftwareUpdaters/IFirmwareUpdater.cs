// Copyright © Svetoslav Paregov. All rights reserved.

namespace NetworkController.SoftwareUpdaters
{
    public interface IFirmwareUpdater
    {
        public bool UpdateLowLevelController(byte[]? firmwareData);
    }
}
