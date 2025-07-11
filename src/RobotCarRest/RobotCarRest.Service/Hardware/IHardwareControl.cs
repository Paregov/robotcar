// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using Paregov.RobotCar.Rest.Service.Models.LowLevel;

namespace Paregov.RobotCar.Rest.Service.Hardware
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
