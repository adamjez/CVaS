using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CVaS.Shared.Services.File
{
    public class AlgorithmFileProvider
    {
        private readonly IConfigurationRoot configuration;
        private readonly ILogger<AlgorithmFileProvider> _logger;

        public AlgorithmFileProvider(IConfigurationRoot configuration, ILogger<AlgorithmFileProvider> logger)
        {
            this.configuration = configuration;
            _logger = logger;
        }

        public string GetAlgorithmFilePath(string codeName, string algFile)
        {
            var pathToFolder = Path.Combine(configuration["DirectoryPaths:Algorithm"], codeName);
            var pathToFile = Path.Combine(pathToFolder, algFile);

            _logger.LogInformation("Creating file to path: " + pathToFile);

            return pathToFile;
        }
    }
}
