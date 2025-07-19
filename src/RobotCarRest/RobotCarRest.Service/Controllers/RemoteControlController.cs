// Copyright © Svetoslav Paregov. All rights reserved.

using System.Threading;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Paregov.RobotCar.Rest.Service.BusinessLogic.Interfaces;
using Paregov.RobotCar.Rest.Service.Hardware;
using Paregov.RobotCar.Rest.Service.Models;
using Paregov.RobotCar.Rest.Service.Models.LowLevel;

namespace Paregov.RobotCar.Rest.Service.Controllers;

[ApiController]
public class RemoteControlController : ControllerBase
{
    private const int s_dataIndexBaseRotation = 0;
    private const int s_dataIndexShoulder = 1;
    private const int s_dataIndexElbow = 2;
    private const int s_dataIndexArm = 3;
    private const int s_dataIndexWrist = 4;
    private const int s_dataIndexGripper = 5;

    private const int s_dataIndexForwardReverse = 6;
    private const int s_dataIndexLeftRight = 7;

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
        var dcMotorSpeed = joysticksRequest.Joysticks[s_dataIndexForwardReverse];
        var dcMotorDirection = joysticksRequest.Joysticks[s_dataIndexLeftRight];

        var baseRotation = joysticksRequest.Joysticks[s_dataIndexBaseRotation];
        var shoulder = joysticksRequest.Joysticks[s_dataIndexShoulder];
        var elbow = joysticksRequest.Joysticks[s_dataIndexElbow];
        var arm = joysticksRequest.Joysticks[s_dataIndexArm];
        var wrist = joysticksRequest.Joysticks[s_dataIndexWrist];
        var gripper = joysticksRequest.Joysticks[s_dataIndexGripper];

        _logger.LogInformation("Received joystick readings: {Readings}", joysticksRequest.Joysticks);

        var (leftMotor, rightMotor) = _motorSpeed.GetDcMotorSpeeds(dcMotorSpeed, dcMotorDirection);

        var directionAndSpeedAllMotorsCommand = new DirectionAndSpeedAllMotorsCommand
        {
            LeftWheel = leftMotor,
            RightWheel = rightMotor,

            Base = DirectionAndSpeedMotorCommand.FromReading(baseRotation),
            Shoulder = DirectionAndSpeedMotorCommand.FromReading(shoulder),
            Elbow = DirectionAndSpeedMotorCommand.FromReading(elbow),
            Arm = DirectionAndSpeedMotorCommand.FromReading(arm),
            Wrist = DirectionAndSpeedMotorCommand.FromReading(wrist),
            Gripper = DirectionAndSpeedMotorCommand.FromReading(gripper)
        };

        var json = System.Text.Json.JsonSerializer.Serialize(directionAndSpeedAllMotorsCommand);
        _logger.LogInformation("Direction and speed all motors command JSON: {Json}", json);

        _hardwareControl.SendDirectionAndSpeedAllMotorsCommand(directionAndSpeedAllMotorsCommand);

        return Ok(new CommandResponse());
    }
}
