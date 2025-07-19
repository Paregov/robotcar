// Copyright © Svetoslav Paregov. All rights reserved.

using System.Linq;
using System.Threading;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Paregov.RobotCar.Rest.Service.Models;
using Paregov.RobotCar.Rest.Service.Servos;

namespace Paregov.RobotCar.Rest.Service.Controllers;

[ApiController]
public class ServosController : ControllerBase
{
    private readonly ILogger<ServosController> _logger;
    private readonly IServos _servos;

    public ServosController(ILogger<ServosController> logger, IServos servos)
    {
        _logger = logger;
        _servos = servos;
    }

    [ApiVersion("1.0")]
    [HttpGet("api/v{version:apiVersion}/servos")]
    public ActionResult<CommandResponse> GetServosStatus(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting servos status");
        return Ok(new CommandResponse());
    }

    [ApiVersion("1.0")]
    [HttpGet("api/v{version:apiVersion}/servos/{servoId:int}/position")]
    public ActionResult<ServoPositionResponse> GetServoPosition(
        [FromRoute] int servoId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting position for servo {ServoId}", servoId);

        if (servoId < 0 || servoId > 255)
        {
            return BadRequest(
                new ServoPositionResponse
                {
                    IsSuccess = false,
                    ServoId = servoId,
                    Details = "Servo ID must be between 0 and 255",
                });
        }

        var position = _servos.GetServoPositionDegrees(servoId);
        return Ok(new ServoPositionResponse
        {
            IsSuccess = true,
            ServoId = servoId,
            CurrentPositionDegrees = position
        });
    }

    /// <summary>
    /// Sets the position of a specific servo with control parameters.
    /// </summary>
    /// <param name="servoId">The servo ID (0-255)</param>
    /// <param name="request">The servo position request with control parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Servo position response</returns>
    [ApiVersion("1.0")]
    [HttpPut("api/v{version:apiVersion}/servos/{servoId:int}/position")]
    public ActionResult<ServoPositionResponse> SetServoPosition(
        [FromRoute] int servoId,
        [FromBody] SetServoPositionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting position for servo {ServoId} to {Position}°", servoId, request.PositionDegrees);

        // Validate route parameter matches body parameter
        if (servoId != request.ServoId)
        {
            return BadRequest(new ServoPositionResponse
            {
                IsSuccess = false,
                ServoId = servoId,
                Details = "Servo ID in route must match servo ID in request body"
            });
        }

        // Validate model state
        if (!ModelState.IsValid)
        {
            return BadRequest(new ServoPositionResponse
            {
                IsSuccess = false,
                ServoId = servoId,
                Details = "Invalid request parameters"
            });
        }

        var success = _servos.SetServoPositionWithTimeout(
            request.ServoId,
            request.PositionDegrees,
            request.AccelerationDegreesPerSecSquared,
            request.VelocityDegreesPerSecond,
            request.TimeoutMs);

        if (success)
        {
            _logger.LogInformation("Successfully set servo {ServoId} position to {Position}°", servoId, request.PositionDegrees);
            return Ok(new ServoPositionResponse
            {
                IsSuccess = true,
                ServoId = servoId,
                CurrentPositionDegrees = request.PositionDegrees,
                Details = "Servo position set successfully"
            });
        }

        _logger.LogWarning("Failed to set servo {ServoId} position to {Position}°", servoId, request.PositionDegrees);
        return StatusCode(500, new ServoPositionResponse
        {
            IsSuccess = false,
            ServoId = servoId,
            Details = "Failed to set servo position"
        });
    }

    /// <summary>
    /// Sets positions for multiple servos simultaneously or sequentially.
    /// </summary>
    /// <param name="request">The multiple servos request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Command response</returns>
    [ApiVersion("1.0")]
    [HttpPut("api/v{version:apiVersion}/servos/positions")]
    public ActionResult<CommandResponse> SetMultipleServoPositions(
        [FromBody] SetMultipleServosRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting positions for {Count} servos {Mode}", 
            request.ServoCommands.Count, request.ExecuteSimultaneously ? "simultaneously" : "sequentially");

        // Validate model state
        if (!ModelState.IsValid)
        {
            return BadRequest(new CommandResponse
            {
                IsSuccess = false
            });
        }

        // Convert List to List of tuples format expected by service using LINQ
        var servoCommandsList = request.ServoCommands.Select(cmd => (
            cmd.ServoId,
            cmd.PositionDegrees,
            cmd.AccelerationDegreesPerSecSquared,
            cmd.VelocityDegreesPerSecond,
            cmd.TimeoutMs
        )).ToList();

        var success = _servos.SetMultipleServoPositions(servoCommandsList, request.ExecuteSimultaneously);

        if (success)
        {
            _logger.LogInformation("Successfully set positions for {Count} servos", request.ServoCommands.Count);
            return Ok(new CommandResponse
            {
                IsSuccess = true
            });
        }

        _logger.LogWarning("Failed to set positions for some servos");
        return StatusCode(500, new CommandResponse
        {
            IsSuccess = false
        });
    }

    /// <summary>
    /// Sets all servos to their center position.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Command response</returns>
    [ApiVersion("1.0")]
    [HttpPut("api/v{version:apiVersion}/servos/center")]
    public ActionResult<CommandResponse> SetAllServosToCenter(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting all servos to center position");

        var success = _servos.SetAllServosToCenter();

        if (success)
        {
            _logger.LogInformation("Successfully centered all servos");
            return Ok(new CommandResponse
            {
                IsSuccess = true
            });
        }

        _logger.LogWarning("Failed to center all servos");
        return StatusCode(500, new CommandResponse
        {
            IsSuccess = false
        });
    }
}
