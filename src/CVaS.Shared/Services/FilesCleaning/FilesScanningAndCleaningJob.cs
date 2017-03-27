using System;
using System.IO;
using CVaS.Shared.Options;
using CVaS.Shared.Services.File.Temporary;
using FluentScheduler;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CVaS.Shared.Services.FilesCleaning
{
    public class FilesScanningAndCleaningJob : IJob
    {
        private readonly ILogger<FilesScanningAndCleaningJob> _logger;
        private readonly ITemporaryFileProvider _temporaryFileProvider;
        private readonly IOptions<FilesCleaningOptions> _options;

        private readonly object _executionLock = new object();
        private readonly double _pressureRetentionInMinutes = 1;

        public FilesScanningAndCleaningJob(ILogger<FilesScanningAndCleaningJob> logger, IOptions<FilesCleaningOptions> options, ITemporaryFileProvider temporaryFileProvider)
        {
            _options = options;
            _temporaryFileProvider = temporaryFileProvider;
            _logger = logger;
        }

        public void Execute()
        {
            lock (_executionLock)
            {
                var temporaryDirectoryPath = _temporaryFileProvider.ResolveTemporaryPath();
                var temporaryDirectoryInfo = new DirectoryInfo(temporaryDirectoryPath);

                if (!temporaryDirectoryInfo.Exists)
                {
                    _logger.LogError("Cannot proceed file scanning becouse temporary directory don't exists");
                    return;
                }

                var currentDrive = new DriveInfo(Directory.GetDirectoryRoot(temporaryDirectoryPath));
                var driveFreeSpacePressure = currentDrive.AvailableFreeSpace < 1000 * 1000 * _options.Value.DrivePressureSpaceInMB;

                if (driveFreeSpacePressure)
                {
                    _logger.LogWarning("Running FilesScanningAndCleaningJob under memory pressure, AvailableFreeSpace=" + currentDrive.AvailableFreeSpace);
                }

                var directorySize = CheckAllCachedFiles(temporaryDirectoryInfo, driveFreeSpacePressure);

                if (directorySize > (_options.Value.DirectoryMaxSpaceInMB * 1000 * 1000))
                {
                    _logger.LogWarning("Running FilesScanningAndCleaningJob under memory pressure, directorySize=" + directorySize);
                    CheckAllCachedFiles(temporaryDirectoryInfo, true);
                }

                _logger.LogInformation("Finished running FilesScanningAndCleaningJob");
            }
        }

        private long CheckAllCachedFiles(DirectoryInfo temporaryDirectoryInfo, bool pressure)
        {
            var currentTimeUtc = DateTime.UtcNow;
            var fileCacheRetentionTimeSpan = TimeSpan.FromMinutes(pressure ? _pressureRetentionInMinutes : _options.Value.FileCacheRetentionTimeInMinutes);
            var sumFilesSize = 0L;

            foreach (var file in temporaryDirectoryInfo.EnumerateFiles())
            {
                sumFilesSize += CheckFile(file, pressure, currentTimeUtc, fileCacheRetentionTimeSpan);
            }

            foreach (var directory in temporaryDirectoryInfo.EnumerateDirectories())
            {
                sumFilesSize += CheckDirectory(directory, pressure);
            }

            return sumFilesSize;
        }

        private long CheckFile(FileInfo file, bool pressure, DateTime currentTimeUtc, TimeSpan fileCacheRetentionTimeSpan)
        {
            if (currentTimeUtc - file.LastAccessTimeUtc > fileCacheRetentionTimeSpan)
            {
                _logger.LogInformation($"Deleting Cached File: {file.FullName} (pressure={pressure})");
                try
                {
                    file.Delete();
                }
                catch (Exception exc)
                {
                    _logger.LogWarning("Cannot delete file: " + exc);
                }
                return 0;
            }

            return file.Length;
        }

        private long CheckDirectory(DirectoryInfo directory, bool pressure)
        {
            var fileSizeInDirectory = CheckAllCachedFiles(directory, pressure);
            if (fileSizeInDirectory == 0)
            {
                _logger.LogInformation("Deleting Empty Folder File: " + directory.FullName);

                try
                {
                    directory.Delete(true);
                }
                catch (Exception exc)
                {
                    _logger.LogWarning("Cannot delete file: " + exc);
                }
            }

            return fileSizeInDirectory;
        }
    }
}