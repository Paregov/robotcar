// Copyright © Svetoslav Paregov. All rights reserved.


using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Logging;

namespace NetworkController.ZipUtilities;

public class UnzipUtility : IUnzipUtility
{
    private readonly ILogger<UnzipUtility> _logger;

    public UnzipUtility(
        ILogger<UnzipUtility> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extracts a zip archive to the same directory where the archive is located.
    /// </summary>
    /// <param name="zipFilePath">The full path to the .zip file.</param>
    public bool UnzipToCurrentFolder(string zipFilePath)
    {
        if (!File.Exists(zipFilePath))
        {
            return false;
        }
        
        string? extractPath = Path.GetDirectoryName(zipFilePath);
        
        if (string.IsNullOrEmpty(extractPath))
        {
            // Fallback to the current working directory of the application if the path is minimal.
            extractPath = Directory.GetCurrentDirectory();
        }

        _logger.LogInformation($"Extracting '{Path.GetFileName(zipFilePath)}' to '{extractPath}'...");
        
        ZipFile.ExtractToDirectory(zipFilePath, extractPath, true);

        _logger.LogInformation("Extraction complete.");

        return true;
    }
}
