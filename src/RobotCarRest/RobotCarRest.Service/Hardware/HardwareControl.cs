// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using Microsoft.Extensions.Logging;
using NetworkController.Models.Enums;
using Paregov.RobotCar.Rest.Service.Hardware.Communication;
using Paregov.RobotCar.Rest.Service.Models.LowLevel;

namespace Paregov.RobotCar.Rest.Service.Hardware
{
    public class HardwareControl : IHardwareControl
    {
        private readonly ILogger<HardwareControl> _logger;
        private readonly IHardwareCommunication _hardwareCommunication;
        private readonly UartCommunication _uartCommunication;

        private readonly object _lock = new();
        private bool _normalOperationsAllowed;

        public HardwareControl(
            ILogger<HardwareControl> logger,
            IHardwareCommunication hardwareCommunication,
            UartCommunication uartCommunication)
        {
            _logger = logger;
            _hardwareCommunication = hardwareCommunication;
            _uartCommunication = uartCommunication;
            _normalOperationsAllowed = true;
        }

        public string SendStringCommandWithResponse(string command)
        {
            if (!_normalOperationsAllowed)
            {
                _logger.LogWarning("Normal operations are not allowed. Command will not be sent: {Command}", command);
                return string.Empty;
            }

            lock (_lock)
            {
                _logger.LogInformation("Sending command: {Command}", command);
                _hardwareCommunication.SendMessage(command); // Use hardware communicator to send the command

                return string.Empty;
            }
        }

        public bool Send8ByteCommand(byte[] command)
        {
            if (!_normalOperationsAllowed)
            {
                _logger.LogWarning("Normal operations are not allowed. Command will not be sent: {Command}", command);
                return false;
            }

            lock (_lock)
            {
                _hardwareCommunication.SendBytesMessage(command);

                return true;
            }
        }

        public bool SendLowLevelCommand(LowLevelCommand command)
        {
            if (!_normalOperationsAllowed)
            {
                _logger.LogWarning("Normal operations are not allowed. Command will not be sent: {Command}", command);
                return false;
            }

            lock (_lock)
            {
                var baseRotation = new CommandData8Bytes(CommandType.BaseMotorCommand, command.Base);
                SendCommandData(baseRotation);

                var shoulder = new CommandData8Bytes(CommandType.ShoulderMotorCommand, command.Shoulder);
                SendCommandData(shoulder);

                var elbow = new CommandData8Bytes(CommandType.ElbowMotorCommand, command.Elbow);
                SendCommandData(elbow);

                var arm = new CommandData8Bytes(CommandType.ArmMotorCommand, command.Arm);
                SendCommandData(arm);

                var wrist = new CommandData8Bytes(CommandType.WristMotorCommand, command.Wrist);
                SendCommandData(wrist);

                var gripper = new CommandData8Bytes(CommandType.GripperMotorCommand, command.Gripper);
                SendCommandData(gripper);

                var leftMotor = new CommandData8Bytes(CommandType.LeftMotorCommand, command.LeftWheel);
                SendCommandData(leftMotor);

                var rightMotor = new CommandData8Bytes(CommandType.RightMotorCommand, command.RightWheel);
                SendCommandData(rightMotor);

                return true;
            }
        }

        private bool SendCommandData(CommandData8Bytes commandData)
        {
            if (!_normalOperationsAllowed)
            {
                _logger.LogWarning(
                    "Normal operations are not allowed. Command will not be sent: {CommandData}",
                    commandData);
                return false;
            }
            lock (_lock)
            {
                var commandBytes = new byte[8];
                commandBytes[0] = commandData.CommandType; // Command type
                Array.Copy(
                    commandData.Data,
                    0,
                    commandBytes,
                    1,
                    commandData.Data.Length);

                _logger.LogInformation($"Sending 8 byte command: [{string.Join(", ", commandBytes)}]");
                _hardwareCommunication.SendBytesMessage(commandBytes);

                return true;
            }
        }

        public bool PrepareForFirmwareUpdate()
        {
            _normalOperationsAllowed = false;
            _uartCommunication.Close();

            return true;
        }

        public bool ResumeAfterFirmwareUpdate()
        {
            _uartCommunication.Open();
            _normalOperationsAllowed = true;

            return true;
        }

        public void Dispose()
        {
            _uartCommunication?.Close();
        }
    }
}
