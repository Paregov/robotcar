﻿// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using NetworkController.Hardware;

namespace NetworkController.SoftwareUpdaters;

public class FirmwareUpdater : IFirmwareUpdater
{
    private readonly ILogger<FirmwareUpdater> _logger;
    private readonly IHardwareControl _hardwareControl;

    private const string FirmwarePath = "/home/paregov/robotcar/firmware/LowLevelController.bin";
    private const string SerialPort = "/dev/serial0";

    private const int BOOT_ENABLE_PIN = 23;
    private const int RUN_CTRL_PIN = 24;

    public FirmwareUpdater(
        ILogger<FirmwareUpdater> logger,
        IHardwareControl hardwareControl)
    {
        _logger = logger;
        _hardwareControl = hardwareControl;
    }

    public bool UpdateLowLevelController(byte[]? firmwareData)
    {
        if (WriteFirmwareToFile(firmwareData))
        {
            try
            {
                _hardwareControl.PrepareForFirmwareUpdate();

                return RunUpdateProcess();
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
    
    private bool RunUpdateProcess()
    {
        var _gpioController = new GpioController();
        
        _gpioController.OpenPin(BOOT_ENABLE_PIN, PinMode.Output);
        _gpioController.OpenPin(RUN_CTRL_PIN, PinMode.Output);

        _logger.LogInformation("GPIO pins initialized.");

        _logger.LogInformation("Starting update_firmware.cs ...");

        _logger.LogInformation("Set the boot enable pin to LOW (bootloader mode).");
        _gpioController.Write(BOOT_ENABLE_PIN, PinValue.Low);

        _logger.LogInformation("Reset low level controller (set RUN_CTRL_PIN to LOW).");
        _gpioController.Write(RUN_CTRL_PIN, PinValue.Low);
        _logger.LogInformation("Wait for 1 second for controller to reset.");
        Thread.Sleep(1000); // 1000 milliseconds = 1 second
        _logger.LogInformation("Start the controller again (set RUN_CTRL_PIN to HIGH).");
        _gpioController.Write(RUN_CTRL_PIN, PinValue.High);

        var result = UpdateUart();
        // var result = UpdateSpi();

        _logger.LogInformation("Set the boot enable pin to HIGH (normal operation mode).");
        _gpioController.Write(BOOT_ENABLE_PIN, PinValue.High);

        //_logger.LogInformation("Reset pins to inputs.");
        // Set pins back to input mode. This releases control of the pins.
        //_gpioController.SetPinMode(BOOT_ENABLE_PIN, PinMode.Input);
        //_gpioController.SetPinMode(RUN_CTRL_PIN, PinMode.Input);

        // Dispose the GpioController when done to release resources
        _gpioController.Dispose();
        _logger.LogInformation("GPIO pins released.");

        _logger.LogInformation($"Firmware update process completed. Success = {result}.");

        return result;
    }

    private bool WriteFirmwareToFile(byte[]? firmwareData)
    {
        try
        {
            // Write the firmware data to the specified file path
            File.WriteAllBytes(FirmwarePath, firmwareData);
            _logger.LogInformation($"Firmware written to {FirmwarePath} successfully.");
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
        string arguments = $"-f \"{FirmwarePath}\" -p \"{SerialPort}\" -a";

        return ExecutePicoboot3(arguments);
    }

    /// <summary>
    /// Executes the picoboot3 command for SPI firmware update.
    /// </summary>
    private bool UpdateSpi()
    {
        _logger.LogInformation("\nStarting SPI firmware update...");

        // Construct the arguments string
        string arguments = $"-i spi -f \"{FirmwarePath}\" --bus 0 --device 0 --baud 10000000 -a";

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
            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();

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
