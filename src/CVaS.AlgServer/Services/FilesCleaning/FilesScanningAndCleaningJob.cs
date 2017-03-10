using FluentScheduler;
using Microsoft.Extensions.Logging;

namespace CVaS.AlgServer.Services.FilesCleaning
{
    public class FilesScanningAndCleaningJob : IJob
    {
        private readonly ILogger<FilesScanningAndCleaningJob> _logger;
        private readonly object _lock = new object();

        public FilesScanningAndCleaningJob(ILogger<FilesScanningAndCleaningJob> logger)
        {
            _logger = logger;
        }

        public void Execute()
        {
            lock (_lock)
            {
                _logger.LogInformation("Running Sheduled Job");
            }
        }
    }
}