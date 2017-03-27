using CVaS.Shared.Options;
using FluentScheduler;
using Microsoft.Extensions.Options;

namespace CVaS.AlgServer.Services.FilesCleaning
{
    public class PeriodFilesCleaningRegistry : Registry
    {
        public PeriodFilesCleaningRegistry(IOptions<FilesCleaningOptions> options)
        {
            // Schedule an IJob to run at an interval
            Schedule<FilesScanningAndCleaningJob>().ToRunNow().AndEvery(options.Value.PeriodInMinutes).Minutes();
        }
    }
}