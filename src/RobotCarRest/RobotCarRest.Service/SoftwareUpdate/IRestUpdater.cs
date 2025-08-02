// Copyright © Svetoslav Paregov. All rights reserved.

using System;

namespace Paregov.RobotCar.Rest.Service.SoftwareUpdate
{
    public interface IRestUpdater
    {
        public bool UpdateRestSoftware(byte[]? archive, string version);

        public Version? GetCurrentVersion();
    }
}
