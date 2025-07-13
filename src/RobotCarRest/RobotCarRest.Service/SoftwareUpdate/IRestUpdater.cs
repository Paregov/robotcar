// Copyright © Svetoslav Paregov. All rights reserved.

namespace Paregov.RobotCar.Rest.Service.SoftwareUpdate
{
    public interface IRestUpdater
    {
        public bool UpdateRestSoftware(byte[]? archive, string version);
    }
}
