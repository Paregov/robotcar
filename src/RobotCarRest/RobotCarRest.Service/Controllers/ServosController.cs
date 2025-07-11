// Copyright © Svetoslav Paregov. All rights reserved.

using System.Threading;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Paregov.RobotCar.Rest.Service.Models;

namespace Paregov.RobotCar.Rest.Service.Controllers;

[ApiController]
public class ServosController : ControllerBase
{
    private readonly ILogger<ServosController> _logger;

    public ServosController(ILogger<ServosController> logger)
    {
        _logger = logger;
    }

    [ApiVersion("1.0")]
    [HttpGet("api/v{version:apiVersion}/servos")]
    public ActionResult<CommandResponse> GetServosStatus(
        CancellationToken cancellationToken = default)
    {
        return Ok(new CommandResponse());
    }

    [ApiVersion("1.0")]
    [HttpPost("api/v{version:apiVersion}/servos")]
    public ActionResult<CommandResponse> SetServos(
        [FromBody]CommandRequest commandRequest,
        CancellationToken cancellationToken = default)
    {
        return Ok(new CommandResponse());
    }
}
