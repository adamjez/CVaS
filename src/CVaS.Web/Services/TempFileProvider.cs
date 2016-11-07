using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace CVaS.Web.Services
{
    public class TempFileProvider
    {
        private readonly IFileProvider fileProvider;
        private readonly IConfigurationRoot configuration;

        public TempFileProvider(IFileProvider fileProvider, IConfigurationRoot configuration)
        {
            this.fileProvider = fileProvider;
            this.configuration = configuration;
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

        public string CreateTempFolder()
        {
            var folderName = Path.GetRandomFileName();
            var directory = Directory.CreateDirectory(folderName);
          
            return directory.FullName;
        }

        public FileStream CreateTempFile1(string fileName)
        {
            return File.Create(fileName);
        }

        public string CreateFileName()
        {
            return Path.GetTempFileName();
        }
    }
}