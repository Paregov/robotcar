// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Paregov.RobotCar.Rest.Service.Hardware;

namespace Paregov.RobotCar.Rest.Service.SoftwareUpdate;

public class FirmwareUpdater : IFirmwareUpdater
{
    private readonly ILogger<FirmwareUpdater> _logger;
    private readonly IHardwareControl _hardwareControl;

    private const string s_serialPort = "/dev/serial0";
    private readonly string _firmwarePath;

    private static readonly int BOOT_ENABLE_PIN = 23;
    private static readonly int RUN_CTRL_PIN = 24;

    public FirmwareUpdater(
        ILogger<FirmwareUpdater> logger,
        IHardwareControl hardwareControl)
    {
        _logger = logger;
        _hardwareControl = hardwareControl;

        _firmwarePath = Path.Join(AppContext.BaseDirectory, "firmware/LowLevelController.bin");
    }

    public bool UpdateLowLevelController(
        byte[]? firmwareData,
        FirmwareUpdateInterface updateInterface = FirmwareUpdateInterface.Uart)
    {
        if (WriteFirmwareToFile(firmwareData))
        {
            try
            {
                _hardwareControl.PrepareForFirmwareUpdate();

                return RunUpdateProcess(updateInterface);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"An error occurred during the firmware update process: {e.Message}");
            }
            finally
            {
                _hardwareControl.ResumeAfterFirmwareUpdate();
            }
        }

        _logger.LogError("Failed to write firmware data to file. Update process aborted.");
        return false;
    }

    private bool RunUpdateProcess(FirmwareUpdateInterface updateInterface = FirmwareUpdateInterface.Uart)
    {
        var _gpioController = new GpioController();

        _gpioController.OpenPin(BOOT_ENABLE_PIN, PinMode.Output);
        _gpioController.OpenPin(RUN_CTRL_PIN, PinMode.Output);

        _logger.LogInformation("GPIO pins initialized.");

        _logger.LogInformation($"Starting firmware update using {updateInterface} interface...");

        _logger.LogInformation("Set the boot enable pin to LOW (bootloader mode).");
        _gpioController.Write(BOOT_ENABLE_PIN, PinValue.Low);

        _logger.LogInformation("Reset low level controller (set RUN_CTRL_PIN to LOW).");
        _gpioController.Write(RUN_CTRL_PIN, PinValue.Low);
        _logger.LogInformation("Wait for 1 second for controller to reset.");
        Thread.Sleep(1000); // 1000 milliseconds = 1 second
        _logger.LogInformation("Start the controller again (set RUN_CTRL_PIN to HIGH).");
        _gpioController.Write(RUN_CTRL_PIN, PinValue.High);

        bool result = updateInterface switch
        {
            FirmwareUpdateInterface.Spi => UpdateSpi(),
            FirmwareUpdateInterface.Uart => UpdateUart(),
            _ => UpdateUart() // Default to UART
        };

        _logger.LogInformation("Set the boot enable pin to HIGH (normal operation mode).");
        _gpioController.Write(BOOT_ENABLE_PIN, PinValue.High);

        // Dispose the GpioController when done to release resources
        _gpioController.Dispose();
        _logger.LogInformation("GPIO pins released.");

        _logger.LogInformation($"Firmware update process completed using {updateInterface} interface. Success = {result}.");

        return result;
    }

    private bool WriteFirmwareToFile(byte[]? firmwareData)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_firmwarePath) ?? string.Empty);

            // Write the firmware data to the specified file path
            File.WriteAllBytes(_firmwarePath, firmwareData);
            _logger.LogInformation($"Firmware written to {_firmwarePath} successfully.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to write firmware to file: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Executes the picoboot3 command for UART firmware update.
    /// </summary>
    private bool UpdateUart()
    {
        _logger.LogInformation("\nStarting UART firmware update...");

        // Construct the arguments string
        var arguments = $"-f \"{_firmwarePath}\" -p \"{s_serialPort}\" -a";

        return ExecutePicoboot3(arguments);
    }

    /// <summary>
    /// Executes the picoboot3 command for SPI firmware update.
    /// </summary>
    private bool UpdateSpi()
    {
        _logger.LogInformation("\nStarting SPI firmware update...");

        // Construct the arguments string
        var arguments = $"-i spi -f \"{_firmwarePath}\" --bus 0 --device 0 --baud 10000000 -a";

        return ExecutePicoboot3(arguments);
    }

    /// <summary>
    /// Helper method to execute the picoboot3 command.
    /// </summary>
    /// <param name="arguments">The arguments string to pass to picoboot3.</param>
    private bool ExecutePicoboot3(string arguments)
    {
        try
        {
            // Create a new ProcessStartInfo object
            ProcessStartInfo startInfo = new()
            {
                FileName = "picoboot3", // The command to execute
                Arguments = arguments, // The arguments for the command
                RedirectStandardOutput = true, // Capture standard output
                RedirectStandardError = true,  // Capture standard error
                UseShellExecute = false,       // Do not use the OS shell to start the process
                CreateNoWindow = true          // Do not create a new window for the process
            };

            _logger.LogInformation($"Executing command: {startInfo.FileName} {startInfo.Arguments}");

            using Process process = new() { StartInfo = startInfo };
            process.Start();

            // Read the output and errors
            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();

            process.WaitForExit(); // Wait for the process to complete

            _logger.LogInformation("\n--- picoboot3 Stdout ---");
            _logger.LogInformation(stdout);
            _logger.LogInformation("\n--- picoboot3 Stderr ---");
            _logger.LogInformation(stderr);

            if (process.ExitCode == 0)
            {
                _logger.LogInformation("picoboot3 command executed successfully.");

                return true; // Indicate success if the exit code is zero
            }
            else
            {
                _logger.LogInformation($"Error calling picoboot3. Exit Code: {process.ExitCode}");

                return false; // Indicate failure if the exit code is not zero
            }
        }
        catch (FileNotFoundException)
        {
            _logger.LogInformation("Error: 'picoboot3' command not found. Make sure it's installed and in your system's PATH.");

            return false; // Indicate failure if the command is not found
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"An unexpected error occurred: {ex.Message}");
            _logger.LogInformation(ex.StackTrace);

            return false; // Indicate failure for any other exceptions
        }
    }
}
