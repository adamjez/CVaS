using CVaS.Shared.Services.File.Temporary;
using FluentScheduler;
using Microsoft.Extensions.Logging;

namespace CVaS.AlgServer.Services.FilesCleaning
{
    public class FilesScanningAndCleaningJob : IJob
    {
        private readonly ILogger<FilesScanningAndCleaningJob> _logger;
        private readonly object _lock = new object();
        private readonly TemporaryFileProvider _temporaryFileProvider;

        public FilesScanningAndCleaningJob(ILogger<FilesScanningAndCleaningJob> logger, TemporaryFileProvider temporaryFileProvider)
        {
            _temporaryFileProvider = temporaryFileProvider;
            _logger = logger;
        }

        public void Execute()
        {
            lock (_lock)
            {
                var temporaryDirectory =_temporaryFileProvider.ResolveTemporaryPath();
                var temporaryDirectoryInfo = new System.IO.DirectoryInfo(temporaryDirectory);

                foreach (var file in temporaryDirectoryInfo.EnumerateFiles())
                {
                    
                }

                _logger.LogInformation("Running Sheduled Job");
            }
        }
    }
}