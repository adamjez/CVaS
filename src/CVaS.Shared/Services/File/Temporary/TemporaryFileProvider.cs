using System.IO;
using CVaS.Shared.Options;
using Microsoft.Extensions.Options;

namespace CVaS.Shared.Services.File.Temporary
{
    public class TemporaryFileProvider : ITemporaryFileProvider
    {
        private readonly string temporaryDirectory;

        public TemporaryFileProvider(IOptions<DirectoryPathOptions> directoryPathOptions)
        {
            this.temporaryDirectory = directoryPathOptions.Value.Temporary ?? Path.GetTempPath();
        }

        public string CreateTemporaryFolder()
        {
            var folderName = Path.GetRandomFileName();
            var directory = Directory.CreateDirectory(ResolveTemporaryPath(folderName));
          
            return directory.FullName;
        }

        public FileStream CreateTemporaryFile(string extension, out string filePath)
        {
            var info = CreateTemporaryFilePath(extension);
            filePath = info.FullPath;

            return System.IO.File.Create(filePath);
        }


        public BasicFileInfo CreateTemporaryFilePath(string extesion = "")
        {
            var fileName = Path.GetRandomFileName() + extesion;
            var filePath = ResolveTemporaryPath(fileName);
            return new BasicFileInfo
            {
                FullPath = filePath,
                FileName = fileName
            };
        }

        public string ResolveTemporaryPath(string file)
        {
            return Path.Combine(temporaryDirectory, file);
        }
    }
}