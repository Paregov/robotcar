// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Paregov.RobotCar.Rest.Service.Hardware;
using Paregov.RobotCar.Rest.Service.Models.Enums;
using Paregov.RobotCar.Rest.Service.Models.LowLevel;

namespace Paregov.RobotCar.Rest.Service.Servos
{
    public class Servos : IServos
    {
        private readonly ILogger<Servos> _logger;
        private readonly IHardwareControl _hardwareControl;

        public Servos(
            ILogger<Servos> logger,
            IHardwareControl hardwareControl)
        {
            _logger = logger;
            _hardwareControl = hardwareControl;
        }

        public bool SetAllServosToCenter()
        {
            _logger.LogInformation("Setting all servos to center position");
            return true;
        }

        public bool SetServoPosition(int servoId, float position, float acceleration, float velocity)
        {
            _logger.LogInformation("Setting servo {ServoId} to position {Position} with acceleration {Acceleration} and velocity {Velocity}", 
                servoId, position, acceleration, velocity);
            return true;
        }

        public bool SetServoPositionWithTimeout(
            int servoId,
            float positionDegrees,
            float accelerationDegreesPerSecSquared,
            float velocityDegreesPerSecond,
            int timeoutMs)
        {
            _logger.LogInformation("Setting servo {ServoId} to {Position}° with acceleration {Acceleration}°/s², velocity {Velocity}°/s, timeout {Timeout}ms", 
                servoId, positionDegrees, accelerationDegreesPerSecSquared, velocityDegreesPerSecond, timeoutMs);

            // Validate parameters
            if (servoId < 0 || servoId > 255)
            {
                _logger.LogError("Invalid servo ID: {ServoId}", servoId);
                return false;
            }

            if (positionDegrees < -180 || positionDegrees > 180)
            {
                _logger.LogError("Invalid position: {Position}°. Must be between -180 and 180 degrees", positionDegrees);
                return false;
            }

            var positionCommand = new PositionAndSpeedServoCommand(
                (Int16)positionDegrees,
                50  // Hardcoded acceleration for now
                );


            var command = new CommandData8Bytes(
                CommandType.BaseMotorPositionCommand,
                positionCommand);

            switch (servoId)
            {
                case 0:
                    command.CommandType = (byte)CommandType.BaseMotorPositionCommand;
                    break;

                case 1:
                    command.CommandType = (byte)CommandType.ShoulderMotorPositionCommand;
                    break;

                case 2:
                    command.CommandType = (byte)CommandType.ElbowMotorPositionCommand;
                    break;

                case 3:
                    command.CommandType = (byte)CommandType.ArmMotorPositionCommand;
                    break;

                case 4:
                    command.CommandType = (byte)CommandType.WristMotorPositionCommand;
                    break;

                case 6:
                    command.CommandType = (byte)CommandType.GripperMotorPositionCommand;
                    break;

                default:
                    _logger.LogError("Servo ID {ServoId} is not allowed to be controlled", servoId);
                    return false;
            }

            var result = _hardwareControl.Send8ByteCommand(command);

            if (!result)
            {
                _logger.LogError("Failed to send command to servo {ServoId}", servoId);
                return false;
            }

            _logger.LogInformation("Servo {ServoId} position set successfully", servoId);
            return true;
        }

        public bool SetMultipleServoPositions(
            List<(int servoId, float positionDegrees, float acceleration, float velocity, int timeoutMs)> servoCommands,
            bool executeSimultaneously = true)
        {
            _logger.LogInformation("Setting {Count} servo positions {Mode}",
                servoCommands.Count, executeSimultaneously ? "simultaneously" : "sequentially");

            if (executeSimultaneously)
            {
                // For simultaneous execution, we'll just call each one
                // In a real implementation, you might use parallel processing
                foreach (var cmd in servoCommands)
                {
                    SetServoPositionWithTimeout(cmd.servoId, cmd.positionDegrees, cmd.acceleration, cmd.velocity, cmd.timeoutMs);
                }
            }
            else
            {
                // Execute commands sequentially
                foreach (var cmd in servoCommands)
                {
                    if (!SetServoPositionWithTimeout(cmd.servoId, cmd.positionDegrees, cmd.acceleration, cmd.velocity, cmd.timeoutMs))
                    {
                        _logger.LogError("Failed to set servo {ServoId} position in sequence", cmd.servoId);
                        return false;
                    }
                }
            }
            return true;
        }

        public bool SetServoPosition(int servoId, float position)
        {
            return SetServoPosition(servoId, position, 100.0f, 50.0f);
        }

        public float GetServoPosition(int servoId)
        {
            _logger.LogInformation("Getting servo {ServoId} position", servoId);
            // TODO: Implement actual position reading from hardware
            return 0.0f;
        }

        public float GetServoPositionDegrees(int servoId)
        {
            _logger.LogInformation("Getting servo {ServoId} position in degrees", servoId);

            if (servoId < 0 || servoId > 255)
            {
                _logger.LogError("Invalid servo ID: {ServoId}", servoId);
                return 0.0f;
            }

            // TODO: Implement actual position reading from hardware
            // For now, return a placeholder value
            return 0.0f;
        }
    }
}
