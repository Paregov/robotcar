// Copyright © Svetoslav Paregov. All rights reserved.

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using NetworkController.ZipUtilities;

namespace Paregov.RobotCar.Rest.Service.SoftwareUpdate
{
    public class RestUpdater : IRestUpdater
    {
        private readonly string _baseFolder;
        private const string s_zipFileName = "restserver.zip";
        private readonly ILogger<RestUpdater> _logger;
        private readonly IUnzipUtility _unzipUtility;

        public RestUpdater(
            ILogger<RestUpdater> logger,
            IUnzipUtility unzipUtility)
        {
            _logger = logger;
            _unzipUtility = unzipUtility;

            var dir = Directory.GetParent(AppContext.BaseDirectory);
            if (dir == null)
            {
                throw new DirectoryNotFoundException("Base directory for REST software updater not found.");
            }

            _baseFolder = Directory.GetParent(dir.FullName)!.FullName;
        }

        public bool UpdateRestSoftware(byte[]? archive, string version)
        {
            if (archive == null || archive.Length == 0)
            {
                _logger.LogError("Received empty archive for REST software update.");
                return false;
            }

            var nextVersion = GetNextBuildVersion(_baseFolder);
            var targetDirectory = Path.Combine(_baseFolder, nextVersion);

            Directory.CreateDirectory(targetDirectory);

            File.WriteAllBytes(Path.Combine(targetDirectory, s_zipFileName), archive);

            var zipFilePath = Path.Combine(targetDirectory, s_zipFileName);
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

            var subDirectoryPaths = Directory.GetDirectories(
                baseFolderPath, "*", SearchOption.TopDirectoryOnly);

            // Use LINQ to process the directory names.
            var latestVersion = subDirectoryPaths
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
                var major = latestVersion.Major;
                var minor = latestVersion.Minor;
                var build = Math.Max(0, latestVersion.Build);
                var revision = Math.Max(0, latestVersion.Revision) + 1;

                return new Version(major, minor, build, revision).ToString();
            }
            else
            {
                return "1.0.0.0";
            }
        }
    }
}
