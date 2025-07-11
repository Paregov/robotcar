// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using System.Threading;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Paregov.RobotCar.Rest.Service.BusinessLogic;
using Paregov.RobotCar.Rest.Service.Hardware;
using Paregov.RobotCar.Rest.Service.Models;
using Paregov.RobotCar.Rest.Service.Models.LowLevel;

namespace Paregov.RobotCar.Rest.Service.Controllers;

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
    private readonly IMotorSpeed _motorSpeed;

    public RemoteControlController(
        ILogger<RemoteControlController> logger,
        IHardwareControl hardwareControl,
        IMotorSpeed motorSpeed)
    {
        _logger = logger;
        _hardwareControl = hardwareControl;
        _motorSpeed = motorSpeed;
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

        _logger.LogInformation("Received joystick readings: {Readings}", joysticksRequest.Joysticks);

        var (leftMotor, rightMotor) = _motorSpeed.GetDcMotorSpeeds(dcMotorSpeed, dcMotorDirection);

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
}
