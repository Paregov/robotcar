// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using System.IO.Ports;
using System.Text;
using Microsoft.Extensions.Logging;
using NetworkController.Models.Enums;
using Paregov.RobotCar.Rest.Service.Models.LowLevel;

namespace Paregov.RobotCar.Rest.Service.Hardware
{
    public class HardwareControl : IHardwareControl
    {
        private readonly ILogger<HardwareControl> _logger;
        private readonly ISpiCommunicator _spiCommunicator;

        // Specify the port name (e.g., "/dev/ttyACM0" for USB serial, "/dev/serial0" for GPIO)
        private const string PortName = "/dev/serial0";
        private const int BaudRate = 115200;
        private const Parity Parity = System.IO.Ports.Parity.None;
        private const int DataBits = 8;
        private const StopBits StopBits = System.IO.Ports.StopBits.One;

        private readonly char[] _startBytes = { (char)0xAA, (char)0xBB, (char)0xCC };
        private readonly char[] _endBytes = { (char)0xDD, (char)0xEE, (char)0xFF };

        private readonly SerialPort? _serialPort;
        private readonly object _lock = new();
        private bool _normalOperationsAllowed;

        public HardwareControl(
            ILogger<HardwareControl> logger,
            ISpiCommunicator spiCommunicator)
        {
            _logger = logger;
            _spiCommunicator = spiCommunicator;
            try
            {
                _serialPort = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits);
                _serialPort?.Open();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to init the serial port.");
            }
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
                var fullCommand = BuildCommand(command);
                _logger.LogInformation("Sending command: {Command}", fullCommand);
                _spiCommunicator.SendMessage(command); // Use SPI communicator to send the command

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
                _spiCommunicator.SendBytesMessage(command);

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
                _spiCommunicator.SendBytesMessage(commandBytes);

                return true;
            }
        }

        public bool PrepareForFirmwareUpdate()
        {
            _normalOperationsAllowed = false;
            _serialPort?.Close();

            return true;
        }

        public bool ResumeAfterFirmwareUpdate()
        {
            _serialPort?.Open();
            _normalOperationsAllowed = true;

            return true;
        }

        private string BuildCommand(string command)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(_startBytes);
            stringBuilder.Append(command);
            stringBuilder.Append(_endBytes);

            return stringBuilder.ToString();
        }

        public void Dispose()
        {
            _serialPort?.Close();
        }
    }
}
