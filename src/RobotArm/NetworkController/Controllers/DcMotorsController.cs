// Copyright © Svetoslav Paregov. All rights reserved.

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetworkController.Models;

namespace NetworkController.Controllers;

[ApiController]
public class DcMotorsController : ControllerBase
{
    private readonly ILogger<ServosController> _logger;

    public DcMotorsController(ILogger<ServosController> logger)
    {
        _logger = logger;
    }

    [ApiVersion("1.0")]
    [HttpGet("api/v{version:apiVersion}/dcmotors/status")]
    public ActionResult<CommandResponse> GetStatus()
    {
        return Ok(new CommandResponse());
    }

    [ApiVersion("1.0")]
    [HttpPost("api/v{version:apiVersion}/dcmotors/{motor}")]
    public ActionResult<CommandResponse> SetServo(
        [FromRoute]int motor,
        [FromBody]CommandRequest commandRequest)
    {
        return Ok(new CommandResponse());
    }
}
