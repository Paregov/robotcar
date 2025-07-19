// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using Paregov.RobotCar.Rest.Service.Models.LowLevel;

namespace Paregov.RobotCar.Rest.Service.Hardware
{
    public interface IHardwareControl : IDisposable
    {
        public string SendStringCommandWithResponse(string command);

        public bool Send8ByteCommand(CommandData8Bytes command);

        public bool SendDirectionAndSpeedAllMotorsCommand(DirectionAndSpeedAllMotorsCommand command);

        public bool PrepareForFirmwareUpdate();

        public bool ResumeAfterFirmwareUpdate();
    }
}
