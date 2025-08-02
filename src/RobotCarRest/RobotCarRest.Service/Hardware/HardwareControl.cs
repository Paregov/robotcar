// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using Microsoft.Extensions.Logging;
using Paregov.RobotCar.Rest.Service.Hardware.Communication;
using Paregov.RobotCar.Rest.Service.Models.Enums;
using Paregov.RobotCar.Rest.Service.Models.LowLevel;

namespace Paregov.RobotCar.Rest.Service.Hardware
{
    public class HardwareControl : IHardwareControl
    {
        private readonly ILogger<HardwareControl> _logger;
        private readonly ISpiCommunication _spiCommunication;

        private readonly object _lock = new();
        private bool _normalOperationsAllowed;

        public HardwareControl(
            ILogger<HardwareControl> logger,
            ISpiCommunication spiCommunication)
        {
            _logger = logger;
            _spiCommunication = spiCommunication;
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
                _spiCommunication.SendMessage(command); // Use SPI communicator to send the command

                return string.Empty;
            }
        }

        public bool SendDirectionAndSpeedAllMotorsCommand(DirectionAndSpeedAllMotorsCommand command)
        {
            if (!_normalOperationsAllowed)
            {
                _logger.LogWarning("Normal operations are not allowed. Command will not be sent: {Command}", command);
                return false;
            }

            lock (_lock)
            {
                var baseRotation = new CommandData8Bytes(CommandType.BaseMotorDirectionCommand, command.Base);
                Send8ByteCommand(baseRotation);

                var shoulder = new CommandData8Bytes(CommandType.ShoulderMotorDirectionCommand, command.Shoulder);
                Send8ByteCommand(shoulder);

                var elbow = new CommandData8Bytes(CommandType.ElbowMotorDirectionCommand, command.Elbow);
                Send8ByteCommand(elbow);

                var arm = new CommandData8Bytes(CommandType.ArmMotorCommand, command.Arm);
                Send8ByteCommand(arm);

                var wrist = new CommandData8Bytes(CommandType.WristMotorCommand, command.Wrist);
                Send8ByteCommand(wrist);

                var gripper = new CommandData8Bytes(CommandType.GripperMotorCommand, command.Gripper);
                Send8ByteCommand(gripper);

                var leftMotor = new CommandData8Bytes(CommandType.LeftMotorCommand, command.LeftWheel);
                Send8ByteCommand(leftMotor);

                var rightMotor = new CommandData8Bytes(CommandType.RightMotorCommand, command.RightWheel);
                Send8ByteCommand(rightMotor);

                return true;
            }
        }

        public bool Send8ByteCommand(CommandData8Bytes commandData)
        {
            if (!_normalOperationsAllowed)
            {
                _logger.LogWarning("Normal operations are not allowed. Command will not be sent: {Command}", commandData);
                return false;
            }

            lock (_lock)
            {
                _logger.LogInformation("Sending 8-byte command: {Command}", commandData);
                return _spiCommunication.SendBytesMessage(commandData.ToByteArray());
            }
        }

        public bool PrepareForFirmwareUpdate()
        {
            lock (_lock)
            {
                _normalOperationsAllowed = false;
                _logger.LogInformation("Hardware prepared for firmware update. Normal operations are disabled.");
                return true;
            }
        }

        public bool ResumeAfterFirmwareUpdate()
        {
            lock (_lock)
            {
                _normalOperationsAllowed = true;
                _logger.LogInformation("Hardware resumed after firmware update. Normal operations are enabled.");
                return true;
            }
        }

        public void Dispose()
        {
            // SPI communication is managed by DI container
            _logger.LogInformation("HardwareControl disposed.");
        }
    }
}
