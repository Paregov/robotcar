// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Paregov.RobotCar.Rest.Service.Models;
using Paregov.RobotCar.Rest.Service.SoftwareUpdate;

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
        [FromQuery] string? updateInterface = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Received firmware update request.");

            var contentLength = Request.ContentLength;

            // Validate content length
            if (contentLength == null || contentLength <= 0)
            {
                const string errorMessage = "Invalid content length. No firmware data provided.";
                _logger.LogError(errorMessage);
                return BadRequest(new CommandResponse
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage,
                    ErrorDetails = $"Content-Length header: {contentLength}"
                });
            }

            using var memoryStream = new MemoryStream();

            try
            {
                await Request.Body.CopyToAsync(memoryStream, cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                const string errorMessage = "Firmware upload was cancelled.";
                _logger.LogError(ex, errorMessage);
                return BadRequest(new CommandResponse
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage,
                    ErrorDetails = ex.Message
                });
            }
            catch (IOException ex)
            {
                const string errorMessage = "Failed to read firmware data from request body.";
                _logger.LogError(ex, errorMessage);
                return BadRequest(new CommandResponse
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage,
                    ErrorDetails = ex.Message
                });
            }

            // Convert the memory stream to a byte array.
            var firmware = memoryStream.ToArray();

            if (firmware.Length == 0 || firmware.Length != contentLength)
            {
                var errorMessage = $"Firmware data validation failed. Expected {contentLength} bytes, received {firmware.Length} bytes.";
                _logger.LogError(errorMessage);
                return BadRequest(new CommandResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Firmware data validation failed.",
                    ErrorDetails = errorMessage
                });
            }

            // Parse the interface parameter, default to UART if not specified or invalid
            FirmwareUpdateInterface selectedInterface;
            try
            {
                selectedInterface = ParseUpdateInterface(updateInterface);
            }
            catch (Exception ex)
            {
                const string errorMessage = "Failed to parse update interface parameter.";
                _logger.LogError(ex, errorMessage);
                return BadRequest(new CommandResponse 
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage,
                    ErrorDetails = $"Interface: '{updateInterface}', Error: {ex.Message}"
                });
            }

            _logger.LogInformation("Firmware update requested using {SelectedInterface} interface with {FirmwareSize} bytes.",
                selectedInterface, firmware.Length);

            // Perform the firmware update
            bool result;
            try
            {
                result = _firmwareUploader.UpdateLowLevelController(firmware, selectedInterface);
            }
            catch (UnauthorizedAccessException ex)
            {
                const string errorMessage = "Insufficient permissions to perform firmware update.";
                _logger.LogError(ex, errorMessage);
                return StatusCode(500, new CommandResponse
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage,
                    ErrorDetails = $"This operation may require elevated privileges. Details: {ex.Message}"
                });
            }
            catch (InvalidOperationException ex)
            {
                const string errorMessage = "Firmware update operation is not valid in the current state.";
                _logger.LogError(ex, errorMessage);
                return StatusCode(500, new CommandResponse
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage,
                    ErrorDetails = ex.Message
                });
            }
            catch (TimeoutException ex)
            {
                const string errorMessage = "Firmware update operation timed out.";
                _logger.LogError(ex, errorMessage);
                return StatusCode(500, new CommandResponse
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage,
                    ErrorDetails = ex.Message
                });
            }
            catch (Exception ex)
            {
                var errorMessage = $"Unexpected error during firmware update using {selectedInterface} interface.";
                _logger.LogError(ex, errorMessage);
                return StatusCode(500, new CommandResponse
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage,
                    ErrorDetails = $"Exception Type: {ex.GetType().Name}, Message: {ex.Message}"
                });
            }

            if (!result)
            {
                var errorMessage = $"Firmware update failed using {selectedInterface} interface. The update process completed but was not successful.";
                _logger.LogError(errorMessage);
                return StatusCode(500, new CommandResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Firmware update process failed.",
                    ErrorDetails = errorMessage
                });
            }

            _logger.LogInformation("Firmware update completed successfully using {SelectedInterface} interface.", selectedInterface);
            return Ok(new CommandResponse { IsSuccess = true });
        }
        catch (Exception ex)
        {
            // Catch-all for any unexpected exceptions
            const string errorMessage = "An unexpected error occurred during firmware update.";
            _logger.LogError(ex, errorMessage);
            return StatusCode(500, new CommandResponse
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                ErrorDetails = $"Exception Type: {ex.GetType().Name}, Message: {ex.Message}, StackTrace: {ex.StackTrace}"
            });
        }
    }

    [ApiVersion("1.0")]
    [Consumes("application/octet-stream")]
    [HttpPost("api/v{version:apiVersion}/software/restserver/")]
    public async Task<ActionResult<CommandResponse>> UpdateRestServerAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Received REST server update request.");

            var contentLength = Request.ContentLength;

            // Validate content length
            if (contentLength == null || contentLength <= 0)
            {
                const string errorMessage = "Invalid content length. No REST server data provided.";
                _logger.LogError(errorMessage);
                return BadRequest(new CommandResponse
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage,
                    ErrorDetails = $"Content-Length header: {contentLength}"
                });
            }

            using var memoryStream = new MemoryStream();

            try
            {
                await Request.Body.CopyToAsync(memoryStream, cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                const string errorMessage = "REST server upload was cancelled.";
                _logger.LogError(ex, errorMessage);
                return BadRequest(new CommandResponse
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage,
                    ErrorDetails = ex.Message
                });
            }
            catch (IOException ex)
            {
                const string errorMessage = "Failed to read REST server data from request body.";
                _logger.LogError(ex, errorMessage);
                return BadRequest(new CommandResponse
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage,
                    ErrorDetails = ex.Message
                });
            }

            // Convert the memory stream to a byte array.
            var restServer = memoryStream.ToArray();

            if (restServer.Length == 0 || restServer.Length != contentLength)
            {
                var errorMessage = $"REST server data validation failed. Expected {contentLength} bytes, received {restServer.Length} bytes.";
                _logger.LogError(errorMessage);
                return BadRequest(new CommandResponse
                {
                    IsSuccess = false, 
                    ErrorMessage = "REST server data validation failed.",
                    ErrorDetails = errorMessage
                });
            }

            try
            {
                _restUpdater.UpdateRestSoftware(restServer, "1.0");
                _logger.LogInformation("REST server update initiated successfully with {DataSize} bytes.", restServer.Length);
            }
            catch (UnauthorizedAccessException ex)
            {
                const string errorMessage = "Insufficient permissions to perform REST server update.";
                _logger.LogError(ex, errorMessage);
                return StatusCode(500, new CommandResponse
                {
                    IsSuccess = false, 
                    ErrorMessage = errorMessage,
                    ErrorDetails = $"This operation may require elevated privileges. Details: {ex.Message}"
                });
            }
            catch (InvalidOperationException ex)
            {
                const string errorMessage = "REST server update operation is not valid in the current state.";
                _logger.LogError(ex, errorMessage);
                return StatusCode(500, new CommandResponse
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage,
                    ErrorDetails = ex.Message
                });
            }
            catch (Exception ex)
            {
                const string errorMessage = "Unexpected error during REST server update.";
                _logger.LogError(ex, errorMessage);
                return StatusCode(500, new CommandResponse
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage,
                    ErrorDetails = $"Exception Type: {ex.GetType().Name}, Message: {ex.Message}"
                });
            }

            // We don't want to block the response waiting for the server to exit,
#pragma warning disable CS4014
            ExitRestServerAsync();
#pragma warning restore CS4014

            return Ok(new CommandResponse { IsSuccess = true });
        }
        catch (Exception ex)
        {
            // Catch-all for any unexpected exceptions
            const string errorMessage = "An unexpected error occurred during REST server update.";
            _logger.LogError(ex, errorMessage);
            return StatusCode(500, new CommandResponse
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                ErrorDetails = $"Exception Type: {ex.GetType().Name}, Message: {ex.Message}, StackTrace: {ex.StackTrace}"
            });
        }
    }

    /// <summary>
    /// Parses the update interface string parameter into the corresponding enum value.
    /// </summary>
    /// <param name="interfaceString">The interface string from the query parameter</param>
    /// <returns>The corresponding FirmwareUpdateInterface enum value, defaults to UART</returns>
    private FirmwareUpdateInterface ParseUpdateInterface(string? interfaceString)
    {
        if (string.IsNullOrWhiteSpace(interfaceString))
        {
            _logger.LogInformation("No update interface specified, defaulting to UART.");
            return FirmwareUpdateInterface.Uart;
        }

        if (Enum.TryParse<FirmwareUpdateInterface>(interfaceString, true, out var parsedInterface))
        {
            _logger.LogInformation($"Update interface parsed as: {parsedInterface}");
            return parsedInterface;
        }

        _logger.LogWarning(
            $"Invalid update interface '{interfaceString}' specified, defaulting to UART. Valid options are: {string.Join(", ", Enum.GetNames<FirmwareUpdateInterface>())}");
        return FirmwareUpdateInterface.Uart;
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
