using System.IO;
using Microsoft.Extensions.Configuration;

namespace CVaS.Shared.Services.File
{
    public class AlgorithmFileProvider
    {
        private readonly IConfigurationRoot configuration;

        public AlgorithmFileProvider(IConfigurationRoot configuration)
        {
            this.configuration = configuration;
        }

        public string GetAlgorithmFilePath(string codeName, string algFile)
        {
            var pathToFolder = Path.Combine(configuration["DirectoryPaths:Algorithm"], codeName);
            var pathToFile = Path.Combine(pathToFolder, algFile);
            return pathToFile;
        }
    }
}
