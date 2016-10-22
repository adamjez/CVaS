using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace CVaS.Web.Services
{
    public class AlgorithmFileProvider
    {
        private readonly IFileProvider fileProvider;
        private readonly IConfigurationRoot configuration;

        public AlgorithmFileProvider(IFileProvider fileProvider, IConfigurationRoot configuration)
        {
            this.fileProvider = fileProvider;
            this.configuration = configuration;
        }

        public IDirectoryContents GetAlgorithmDirectoryContents(string subpath = "")
        {
            var path = Path.Combine(configuration["DirectoryPaths:Algorithm"], subpath);
            return fileProvider.GetDirectoryContents(path);
        }
    }
}
