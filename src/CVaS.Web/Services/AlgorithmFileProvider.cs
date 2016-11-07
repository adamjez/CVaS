using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace CVaS.Web.Services
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

    public class FileProvider
    {
        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public bool IsEmpty(string directory)
        {
            return !Directory.EnumerateFiles(directory).Any();
        }

        public string GetDirectoryFromFile(string filePath)
        {
            return Directory.GetParent(filePath).FullName;
        }
    }
}
