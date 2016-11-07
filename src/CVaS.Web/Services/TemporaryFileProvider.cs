using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CVaS.Web.Services
{
    public class TemporaryFileProvider
    {
        private readonly string temporaryDirectory;

        public TemporaryFileProvider(IConfigurationRoot configuration)
        {
            this.temporaryDirectory = configuration["DirectoryPaths:Temporary"];
        }

        public async Task<string> CreateTempFile(IFormFile formFile)
        {
            var path = Path.GetTempFileName();
            using (var tmpFile = File.Create(path))
            {
                await formFile.CopyToAsync(tmpFile);
            }

            return path;
        }

        public string CreateTemporaryFolder()
        {
            var folderName = Path.GetRandomFileName();
            var directory = Directory.CreateDirectory(Path.Combine(temporaryDirectory, folderName));
          
            return directory.FullName;
        }

        public FileStream CreateTemporaryFile(string fileName)
        {
            return File.Create(fileName);
        }

        public string CreateTemporaryFileName()
        {
            return ResolveTemporaryFilePath(Path.GetTempFileName());
        }

        public string ResolveTemporaryFilePath(string file)
        {
            return Path.Combine(temporaryDirectory, file);
        }
    }
}