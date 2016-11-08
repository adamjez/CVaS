using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CVaS.Web.Services
{
    public class TemporaryFileProvider
    {
        private readonly string temporaryDirectory;

        public TemporaryFileProvider()
        {
            this.temporaryDirectory = Path.GetTempPath();
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

        public FileStream CreateTemporaryFile(out string filePath)
        {
            var info = CreateTemporaryFile();
            filePath = info.FullPath;

            return File.Create(filePath);
        }

        public BasicFileInfo CreateTemporaryFile()
        {
            var filePath = Path.GetTempFileName();
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
    }

    public class BasicFileInfo
    {
        public string FileName { get; set; }
        public string FullPath { get; set; }
    }
}