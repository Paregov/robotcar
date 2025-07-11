// Copyright © Svetoslav Paregov. All rights reserved.

namespace Paregov.RobotCar.Rest.Service.SoftwareUpdaters
{
    public interface IRestUpdater
    {
        public bool UpdateRestSoftware(byte[]? archive, string version);
    }
}
