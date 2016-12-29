using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CVaS.BL.Services.File
{
    public class TemporaryFileProvider
    {
        private readonly string temporaryDirectory;

        public TemporaryFileProvider(IConfigurationRoot configuration)
        {
            this.temporaryDirectory = configuration["DirectoryPaths:Temporary"] ?? Path.GetTempPath();
        }

        public async Task<string> CreateTempFile(IFormFile formFile)
        {
            var path = Path.GetTempFileName();
            using (var tmpFile = System.IO.File.Create(path))
            {
                await formFile.CopyToAsync(tmpFile);
            }

            return path;
        }

        public string CreateTemporaryFolder()
        {
            var folderName = Path.GetRandomFileName();
            var directory = Directory.CreateDirectory(ResolveTemporaryFilePath(folderName));
          
            return directory.FullName;
        }

        public FileStream CreateTemporaryFile(string extension, out string filePath)
        {
            var info = GetTemporaryFile(extension);
            filePath = info.FullPath;

            return System.IO.File.Create(filePath);
        }

        public BasicFileInfo GetTemporaryFile(string extesion = "")
        {
            var filePath = ResolveTemporaryFilePath(Path.GetRandomFileName() + extesion);
            return new BasicFileInfo
            {
                FullPath = filePath,
                FileName = Path.GetFileName(filePath)
            };
        }

        public string ResolveTemporaryFilePath(string file)
        {
            return Path.Combine(temporaryDirectory, file);
        }

        public void Delete(string filePath)
        {
            System.IO.File.Delete(filePath);
        }
    }
}