using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Paregov.RobotCar.Rest.Service.Hardware.Communication.Config;
using Paregov.RobotCar.Rest.Service.Hardware.Display;
using Paregov.RobotCar.Rest.Service.SoftwareUpdate;

namespace Paregov.RobotCar.Rest.Service.Worker
{
    /// <summary>
    /// Background service that manages the robot car's display and periodic tasks.
    /// </summary>
    public class RobotCarWorker : BackgroundService
    {
        private readonly ILogger<RobotCarWorker> _logger;
        private readonly IRestUpdater _restUpdater;
        private readonly Ssd1306Display? _display;

        public RobotCarWorker(
            ILogger<RobotCarWorker> logger,
            ILoggerFactory loggerFactory,
            IOptions<Ssd1306Options> ssd1306Options,
            IRestUpdater restUpdater)
        {
            _logger = logger;
            _restUpdater = restUpdater;

            // Create SSD1306 display directly with IOptions
            try
            {
                _display = new Ssd1306Display(
                    loggerFactory.CreateLogger<Ssd1306Display>(), 
                    ssd1306Options);

                if (_display.Initialize())
                {
                    _logger.LogInformation("SSD1306 display initialized successfully in RobotCarWorker");
                }
                else
                {
                    _logger.LogWarning("Failed to initialize SSD1306 display in RobotCarWorker");
                    _display.Dispose();
                    _display = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create SSD1306 display in RobotCarWorker");
                _display = null;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Ensure we yield to allow the host to start properly.
            await Task.Yield();

            _logger.LogInformation("RobotCarWorker starting...");

            try
            {
                // Display IP address and version on startup
                await DisplaySystemInformation();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to display system information on startup");
            }

            // Main worker loop.
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Periodic tasks can be added here.
                    // For now, just update the display every 30 seconds.
                    await Task.Delay(30000, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await UpdateDisplayInformation();
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in RobotCarWorker main loop");
                    await Task.Delay(5000, stoppingToken); // Wait before retrying
                }
            }

            _logger.LogInformation("RobotCarWorker stopping...");
        }

        /// <summary>
        /// Displays system information including IP addresses and version on the SSD1306 display.
        /// Optimized layout for 128x32 display (4 lines of text).
        /// </summary>
        private Task DisplaySystemInformation()
        {
            if (_display == null || !_display.IsInitialized)
            {
                _logger.LogWarning("Display not available for system information");
                return Task.CompletedTask;
            }

            try
            {
                var ipAddresses = GetLocalIPAddresses();
                var currentVersion = GetCurrentVersionString();
                
                _display.Clear();
                
                // Line 1: Title and version (0-7 pixels)
                _display.DrawText(0, 0, $"Robot Car {currentVersion}");
                
                // Line 2: Separator line (8-15 pixels)
                _display.DrawHorizontalLine(0, 9, 128);
                
                int yPos = 12;
                
                // Line 3: IP address or no network (16-23 pixels)  
                if (ipAddresses.Length > 0)
                {
                    // Show only the first IP address due to space constraints
                    var ipText = ipAddresses[0];
                    if (ipText.Length > 21) ipText = ipText.Substring(0, 21); // Fit in 128px
                    _display.DrawText(0, yPos, ipText);
                }
                else
                {
                    _display.DrawText(0, yPos, "No network");
                }
                
                // Line 4: Timestamp (24-31 pixels)
                var timestamp = DateTime.Now.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                _display.DrawText(80, 24, timestamp);
                
                if (_display.Update())
                {
                    _logger.LogInformation("System information displayed on SSD1306 128x32");
                    _logger.LogInformation("Current version: {Version}", currentVersion);
                    
                    // Log IP addresses to console as well
                    foreach (var ip in ipAddresses)
                    {
                        _logger.LogInformation("Local IP Address: {IPAddress}", ip);
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to update display with system information");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to display system information");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Periodically updates display information.
        /// Updates only the timestamp area for 128x32 display.
        /// </summary>
        private Task UpdateDisplayInformation()
        {
            if (_display == null || !_display.IsInitialized)
            {
                return Task.CompletedTask;
            }

            try
            {
                // For now, just update the timestamp
                // In the future, this could show system status, sensor readings, etc.
                var timestamp = DateTime.Now.ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                
                // Clear just the timestamp area and redraw it (line 4: 24-31 pixels)
                for (int x = 80; x < 128; x++)
                {
                    for (int y = 24; y < 32; y++)
                    {
                        _display.SetPixel(x, y, false);
                    }
                }
                
                _display.DrawText(80, 24, timestamp);
                _display.Update();
                
                _logger.LogDebug("Display timestamp updated: {Timestamp}", timestamp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update display information");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the current software version as a string.
        /// </summary>
        /// <returns>Version string or "Unknown" if version cannot be determined</returns>
        private string GetCurrentVersionString()
        {
            try
            {
                var version = _restUpdater.GetCurrentVersion();
                if (version != null)
                {
                    // Format version for display (e.g., "1.2.3.4" -> "v1.2.3.4")
                    return $"v{version}";
                }
                else
                {
                    // Fallback to assembly version if RestUpdater returns null
                    var assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                    if (assemblyVersion != null)
                    {
                        return $"v{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get current version from RestUpdater");
                
                // Try assembly version as fallback
                try
                {
                    var assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                    if (assemblyVersion != null)
                    {
                        return $"v{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";
                    }
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogWarning(fallbackEx, "Failed to get assembly version as fallback");
                }
            }

            return "Unknown";
        }

        /// <summary>
        /// Gets all local IP addresses (excluding loopback).
        /// </summary>
        /// <returns>Array of IP address strings</returns>
        private string[] GetLocalIPAddresses()
        {
            try
            {
                var ipAddresses = new List<string>();

                // Get all network interfaces
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up && 
                                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                foreach (var networkInterface in networkInterfaces)
                {
                    var ipProps = networkInterface.GetIPProperties();
                    foreach (var ipAddr in ipProps.UnicastAddresses)
                    {
                        // Only include IPv4 addresses that are not loopback
                        if (ipAddr.Address.AddressFamily == AddressFamily.InterNetwork && 
                            !IPAddress.IsLoopback(ipAddr.Address))
                        {
                            ipAddresses.Add(ipAddr.Address.ToString());
                        }
                    }
                }

                return ipAddresses.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get local IP addresses");
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Cleanup when the service is stopped.
        /// </summary>
        public override void Dispose()
        {
            try
            {
                if (_display != null)
                {
                    _display.Clear();
                    _display.Update();
                    _display.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RobotCarWorker");
            }

            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
