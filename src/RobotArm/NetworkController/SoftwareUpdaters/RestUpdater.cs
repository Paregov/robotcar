// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using NetworkController.ZipUtilities;

namespace NetworkController.SoftwareUpdaters
{
    public class RestUpdater : IRestUpdater
    {
        private const string BaseFolder = "/home/paregov/robotcar/restserver/";
        private const string ZipFileName = "restserver.zip";
        private readonly ILogger<RestUpdater> _logger;
        private readonly IUnzipUtility _unzipUtility;

        public RestUpdater(
            ILogger<RestUpdater> logger,
            IUnzipUtility unzipUtility)
        {
            _logger = logger;
            _unzipUtility = unzipUtility;
        }

        public bool UpdateRestSoftware(byte[]? archive, string version)
        {
            if (archive == null || archive.Length == 0)
            {
                _logger.LogError("Received empty archive for REST software update.");
                return false;
            }
            
            var nextVersion = GetNextBuildVersion(BaseFolder);
            var targetDirectory = Path.Combine(BaseFolder, nextVersion);
            Directory.CreateDirectory(targetDirectory);

            File.WriteAllBytes(Path.Combine(targetDirectory, ZipFileName), archive);

            var zipFilePath = Path.Combine(targetDirectory, ZipFileName);
            var result = _unzipUtility.UnzipToCurrentFolder(zipFilePath);

            _logger.LogInformation($"Successfully installed REST software to version {nextVersion}.");

            return result;
        }

        private string GetNextBuildVersion(string baseFolderPath)
        {
            if (!Directory.Exists(baseFolderPath))
            {
                throw new DirectoryNotFoundException(
                    $"The specified base folder does not exist: {baseFolderPath}");
            }
            
            var subdirectoryPaths = Directory.GetDirectories(
                baseFolderPath, "*", SearchOption.TopDirectoryOnly);

            // Use LINQ to process the directory names.
            var latestVersion = subdirectoryPaths
                .Select(Path.GetFileName)
                .Select(name =>
                {
                    Version.TryParse(name, out Version? version);
                    return version;
                })
                .Where(version => version != null)
                .MaxBy(v => v);
            
            if (latestVersion != null)
            {
                int major = latestVersion.Major;
                int minor = latestVersion.Minor;
                int build = Math.Max(0, latestVersion.Build);
                int revision = Math.Max(0, latestVersion.Revision) + 1;

                return new Version(major, minor, build, revision).ToString();
            }
            else
            {
                return "1.0.0.0";
            }
        }
    }
}
