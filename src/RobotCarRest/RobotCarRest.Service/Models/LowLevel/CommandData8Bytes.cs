// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using NetworkController.Models.Enums;

namespace Paregov.RobotCar.Rest.Service.Models.LowLevel
{
    public struct CommandData8Bytes
    {
        public CommandData8Bytes(CommandType commandType, SingleMotorCommand command)
        {
            CommandType = (byte)commandType;
            Data[0] = (byte)command.Direction;
            Data[1] = (byte)command.Speed;

            UInt16 timeout = (UInt16)command.TimeOutMilliseconds;
            Data[2] = (byte)(timeout >> 8); // High byte
            Data[3] = (byte)(timeout & 0xFF); // Low byte
            Data[4] = 0x00;
            Data[5] = 0x00;
            Data[6] = 0x00;
        }

        public byte CommandType { get; init; }

        public byte[] Data { get; init; } = new byte[7];
    }
}
