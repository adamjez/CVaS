using CVaS.AlgServer.Options;
using CVaS.Shared.Services.File.Temporary;
using FluentScheduler;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace CVaS.AlgServer.Services.FilesCleaning
{
    public class FilesScanningAndCleaningJob : IJob
    {
        private readonly ILogger<FilesScanningAndCleaningJob> _logger;
        private readonly object _lock = new object();
        private readonly ITemporaryFileProvider _temporaryFileProvider;
        private readonly IOptions<FilesCleaningOptions> _options;
        private readonly double PressureRetentionInMinutes = 1;

        public FilesScanningAndCleaningJob(ILogger<FilesScanningAndCleaningJob> logger, IOptions<FilesCleaningOptions> options, ITemporaryFileProvider temporaryFileProvider)
        {
            _options = options;
            _temporaryFileProvider = temporaryFileProvider;
            _logger = logger;
        }

        public void Execute()
        {
            lock (_lock)
            {
                var temporaryDirectoryPath = _temporaryFileProvider.ResolveTemporaryPath();
                var temporaryDirectoryInfo = new DirectoryInfo(temporaryDirectoryPath);

                if (!temporaryDirectoryInfo.Exists)
                {
                    return;
                }

                var currentDrive = new DriveInfo(Directory.GetDirectoryRoot(temporaryDirectoryPath));
                var driveFreeSpacePressure = currentDrive.AvailableFreeSpace < 1000 * 1000 * _options.Value.DrivePressureSpaceInMB;

                if (driveFreeSpacePressure)
                {
                    _logger.LogWarning("Running FilesScanningAndCleaningJob under memory pressure, AvailableFreeSpace=" + currentDrive.AvailableFreeSpace);
                }

                var directorySize = CheckAllCachedFilesSlow(temporaryDirectoryInfo, driveFreeSpacePressure);

                if (directorySize > (_options.Value.DirectoryMaxSpaceInMB * 1000 * 1000))
                {
                    _logger.LogWarning("Running FilesScanningAndCleaningJob under memory pressure, directorySize=" + directorySize);
                    CheckAllCachedFilesSlow(temporaryDirectoryInfo, true);
                }

                _logger.LogInformation("Finished running FilesScanningAndCleaningJob");
            }
        }

        private long CheckAllCachedFilesFast(DirectoryInfo temporaryDirectoryInfo, bool pressure)
        {
            var currentTimeUtc = DateTime.UtcNow;
            var fileCacheRetentionTimeSpan = TimeSpan.FromMinutes(pressure ? PressureRetentionInMinutes : _options.Value.FileCacheRetentionTimeInMinutes);
            var sumFilesSize = 0L;
            foreach (var file in temporaryDirectoryInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                if (currentTimeUtc - file.LastAccessTimeUtc > fileCacheRetentionTimeSpan)
                {
                    _logger.LogInformation($"Deleting Cached File: {file.FullName} ({currentTimeUtc - file.LastAccessTimeUtc} - {fileCacheRetentionTimeSpan})");
                    file.Delete();
                    continue;
                }

                sumFilesSize += file.Length;
            }

            return sumFilesSize;
        }

        private long CheckAllCachedFilesSlow(DirectoryInfo temporaryDirectoryInfo, bool pressure)
        {
            var currentTimeUtc = DateTime.UtcNow;
            var fileCacheRetentionTimeSpan = TimeSpan.FromMinutes(pressure ? PressureRetentionInMinutes : _options.Value.FileCacheRetentionTimeInMinutes);
            var sumFilesSize = 0L;

            foreach (var file in temporaryDirectoryInfo.EnumerateFiles())
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
                    continue;
                }

                sumFilesSize += file.Length;
            }

            foreach (var directoryInfo in temporaryDirectoryInfo.EnumerateDirectories())
            {
                var fileSizeInDirectory = CheckAllCachedFilesSlow(directoryInfo, pressure);
                if (fileSizeInDirectory == 0)
                {
                    _logger.LogInformation("Deleting Empty Folder File: " + directoryInfo.FullName);
                    directoryInfo.Delete(true);
                }

                sumFilesSize += fileSizeInDirectory;
            }

            return sumFilesSize;
        }
    }
}