// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using NetworkController.Models.LowLevel;

namespace NetworkController.Hardware
{
    public interface IHardwareControl : IDisposable
    {
        public string SendStringCommandWithResponse(string command);

        public bool Send8ByteCommand(byte[] command);

        public bool SendLowLevelCommand(LowLevelCommand command);

        public bool PrepareForFirmwareUpdate();

        public bool ResumeAfterFirmwareUpdate();
    }
}
