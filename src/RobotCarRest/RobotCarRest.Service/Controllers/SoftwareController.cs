// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Paregov.RobotCar.Rest.Service.Models;
using Paregov.RobotCar.Rest.Service.SoftwareUpdaters;

namespace Paregov.RobotCar.Rest.Service.Controllers;

[ApiController]
public class SoftwareController : ControllerBase
{
    private readonly ILogger<SoftwareController> _logger;
    private readonly IFirmwareUpdater _firmwareUploader;
    private readonly IRestUpdater _restUpdater;

    public SoftwareController(
        ILogger<SoftwareController> logger,
        IFirmwareUpdater firmwareUploader,
        IRestUpdater restUpdater
        )
    {
        _logger = logger;
        _firmwareUploader = firmwareUploader;
        _restUpdater = restUpdater;
    }

    [ApiVersion("1.0")]
    [Consumes("application/octet-stream")]
    [HttpPost("api/v{version:apiVersion}/software/firmware/")]
    public async Task<ActionResult<CommandResponse>> UpdateLowLevelControllerFirmwareAsync(
        CancellationToken cancellationToken = default)
    {
        var contentLength = Request.ContentLength;
        using var memoryStream = new MemoryStream();
        await Request.Body.CopyToAsync(memoryStream, cancellationToken);

        // Convert the memory stream to a byte array.
        byte[] firmware = memoryStream.ToArray();

        if (firmware.Length == 0 || firmware.Length != contentLength)
        {
            _logger.LogError("Firmware data is null or empty.");
            return BadRequest(new CommandResponse { IsSuccess = false });
        }

        var result =_firmwareUploader.UpdateLowLevelController(firmware);
        if (!result)
        {
            _logger.LogError("Failed to update low level controller firmware.");
            return StatusCode(500, new CommandResponse { IsSuccess = false });
        }

        return Ok(new CommandResponse());
    }

    [ApiVersion("1.0")]
    [Consumes("application/octet-stream")]
    [HttpPost("api/v{version:apiVersion}/software/restserver/")]
    public async Task<ActionResult<CommandResponse>> UpdateRestServerAsync(
        CancellationToken cancellationToken = default)
    {
        var contentLength = Request.ContentLength;
        using var memoryStream = new MemoryStream();
        await Request.Body.CopyToAsync(memoryStream, cancellationToken);

        // Convert the memory stream to a byte array.
        byte[] restServer = memoryStream.ToArray();

        if (restServer.Length == 0 || restServer.Length != contentLength)
        {
            _logger.LogError("REST server data is null or empty.");
            return BadRequest(new CommandResponse { IsSuccess = false });
        }

        _restUpdater.UpdateRestSoftware(restServer, "1.0");

        // We don't want to block the response waiting for the server to exit,
#pragma warning disable CS4014
        ExitRestServerAsync();
#pragma warning restore CS4014

        return Ok(new CommandResponse());
    }

    private async Task ExitRestServerAsync()
    {
        // This method is a placeholder for any cleanup or exit logic needed for the REST server.
        // It can be implemented as needed in the future.
        _logger.LogInformation("Sleep for 1 second before exiting the REST server.");
        await Task.Delay(1000);
        _logger.LogInformation("Exiting REST server gracefully.");
        Environment.Exit(0);
    }
}
