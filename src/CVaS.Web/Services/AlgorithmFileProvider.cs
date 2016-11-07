using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace CVaS.Web.Services
{
    public class AlgorithmFileProvider
    {
        private readonly FileProvider fileProvider;
        private readonly IConfigurationRoot configuration;

        public AlgorithmFileProvider(FileProvider fileProvider, IConfigurationRoot configuration)
        {
            this.fileProvider = fileProvider;
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
    }
}
