// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using Paregov.RobotCar.Rest.Service.Models.Enums;

namespace Paregov.RobotCar.Rest.Service.Models.LowLevel
{
    public struct CommandData8Bytes
    {
        public CommandData8Bytes(
            CommandType commandType,
            DirectionAndSpeedMotorCommand command)
        {
            CommandType = (byte)commandType;
            Data[0] = (byte)command.Direction;
            Data[1] = (byte)command.Speed;

            UInt16 timeout = (UInt16)command.TimeOutMilliseconds;
            Data[2] = (byte)(timeout >> 8); // High byte
            Data[3] = (byte)(timeout & 0xFF); // Low byte
            Data[4] = 0x00;  // Reserved
            Data[5] = 0x00;  // Reserved
            Data[6] = 0x00;  // Reserved
        }

        public CommandData8Bytes(
            CommandType commandType,
            PositionAndSpeedServoCommand command)
        {
            CommandType = (byte)commandType;
            Data[0] = (byte)(command.Position >> 8); // High byte
            Data[1] = (byte)(command.Position & 0xFF); // Low byte
            Data[2] = command.Speed; // Speed in percentage
            Data[3] = 0x00; // Reserved
            Data[4] = 0x00; // Reserved
            Data[5] = 0x00; // Reserved
            Data[6] = 0x00; // Reserved
        }

        public byte CommandType { get; set; }

        public byte[] Data { get; init; } = new byte[7];

        /// <summary>
        /// Converts the command data to a byte array for transmission.
        /// </summary>
        /// <returns>An 8-byte array containing the command type and data</returns>
        public readonly byte[] ToByteArray()
        {
            var result = new byte[8];
            result[0] = CommandType;
            Array.Copy(Data, 0, result, 1, 7);
            return result;
        }
    }
}
