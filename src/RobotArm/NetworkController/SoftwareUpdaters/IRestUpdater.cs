// Copyright © Svetoslav Paregov. All rights reserved.

namespace NetworkController.SoftwareUpdaters
{
    public interface IRestUpdater
    {
        public bool UpdateRestSoftware(byte[]? archive, string version);
    }
}
