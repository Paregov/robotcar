// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using System.Threading;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetworkController.Hardware;
using NetworkController.Models;
using NetworkController.Models.LowLevel;

namespace NetworkController.Controllers;

[ApiController]
public class RemoteControlController : ControllerBase
{
    private const int DataIndexBaseRotation = 0;
    private const int DataIndexShoulder = 1;
    private const int DataIndexElbow = 2;
    private const int DataIndexArm = 3;
    private const int DataIndexWrist = 4;
    private const int DataIndexGripper = 5;

    private const int DataIndexForwardReverse = 6;
    private const int DataIndexLeftRight = 7;

    private readonly ILogger<RemoteControlController> _logger;
    private readonly IHardwareControl _hardwareControl;

    public RemoteControlController(
        ILogger<RemoteControlController> logger,
        IHardwareControl hardwareControl)
    {
        _logger = logger;
        _hardwareControl = hardwareControl;
    }

    [ApiVersion("1.0")]
    [HttpPost("api/v{version:apiVersion}/remotecontrol/joysticks")]
    public ActionResult<CommandResponse> SetServos(
        [FromBody] JoysticksRequest joysticksRequest,
        CancellationToken cancellationToken = default)
    {
        if (joysticksRequest.Joysticks.Count != 8)
        {
            _logger.LogError("Joystick readings are not the expected count.");
            return BadRequest(new CommandResponse { IsSuccess = false });
        }

        // Joystick readings:
        // 0 - Base rotation servo.
        // 1 - Shoulder servo.
        // 2 - Elbow servo.
        // 3 - Arm servo.
        // 4 - Wrist servo.
        // 5 - Gripper servo.

        // 6 - DC motors speed and forward/reverse direction.
        // 7 - DC motors left/right direction.

        // We are going to use the values for velocity. 0-127 is reverse, 128-255 is forward.
        var dcMotorSpeed = joysticksRequest.Joysticks[DataIndexForwardReverse];
        var dcMotorDirection = joysticksRequest.Joysticks[DataIndexLeftRight];

        var baseRotation = joysticksRequest.Joysticks[DataIndexBaseRotation];
        var shoulder = joysticksRequest.Joysticks[DataIndexShoulder];
        var elbow = joysticksRequest.Joysticks[DataIndexElbow];
        var arm = joysticksRequest.Joysticks[DataIndexArm];
        var wrist = joysticksRequest.Joysticks[DataIndexWrist];
        var gripper = joysticksRequest.Joysticks[DataIndexGripper];

        _logger.LogInformation("Received joysticke readings: {Readings}", joysticksRequest.Joysticks);

        var (leftMotor, rightMotor) = GetDcMotorSpeeds(dcMotorSpeed, dcMotorDirection);

        var lowLevelCommand = new LowLevelCommand
        {
            LeftWheel = leftMotor,
            RightWheel = rightMotor,

            Base = SingleMotorCommand.FromReading(baseRotation),
            Shoulder = SingleMotorCommand.FromReading(shoulder),
            Elbow = SingleMotorCommand.FromReading(elbow),
            Arm = SingleMotorCommand.FromReading(arm),
            Wrist = SingleMotorCommand.FromReading(wrist),
            Gripper = SingleMotorCommand.FromReading(gripper)
        };

        var json = System.Text.Json.JsonSerializer.Serialize(lowLevelCommand);
        _logger.LogInformation("Low level command JSON: {Json}", json);

        _hardwareControl.SendLowLevelCommand(lowLevelCommand);

        return Ok(new CommandResponse());
    }

    (SingleMotorCommand left, SingleMotorCommand right) GetDcMotorSpeeds(int xValue, int yValue)
    {
        // Convert the 0-255 joystick readings to a -127 to 128 range.
        int xMapped = xValue - 128;
        int yMapped = yValue - 128;
        
        // This creates a small area around the center where the motors won't move.
        if (Math.Abs(xMapped) < 10) xMapped = 0;
        if (Math.Abs(yMapped) < 10) yMapped = 0;


        // Motor Speed Calculation.
        // Forward and backward movement.
        int forwardSpeed = xMapped;
        int turnSpeed = yMapped;

        // Calculate the speed for each motor.
        int leftSpeed = forwardSpeed + turnSpeed;
        int rightSpeed = forwardSpeed - turnSpeed;

        // Constrain the speeds to the -127 to 127 range.
        if (leftSpeed > 127) leftSpeed = 127;
        if (leftSpeed < -127) leftSpeed = -127;
        if (rightSpeed > 127) rightSpeed = 127;
        if (rightSpeed < -127) rightSpeed = -127;

        // Remap to 0-100 for motor output.
        int leftMotorSpeed = (leftSpeed * 100) / 127;
        int rightMotorSpeed = (rightSpeed * 100) / 127;

        var left = new SingleMotorCommand
        {
            Direction = leftMotorSpeed < 0 ? -1 : (leftMotorSpeed > 0 ? 1 : 0),
            Speed = Math.Abs(leftMotorSpeed)
        };

        var right = new SingleMotorCommand
        {
            Direction = rightMotorSpeed < 0 ? -1 : (rightMotorSpeed > 0 ? 1 : 0),
            Speed = Math.Abs(rightMotorSpeed)
        };

        return (left, right);
    }
}
