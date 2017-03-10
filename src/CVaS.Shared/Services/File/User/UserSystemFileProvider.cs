using System.IO;
using System.Threading.Tasks;
using CVaS.Shared.Services.File.Temporary;

namespace CVaS.Shared.Services.File.Providers
{
    public class UserSystemFileProvider : UserFileProvider
    {
        private readonly ITemporaryFileProvider _tmpFileProvider;
        private readonly FileSystemWrapper _fileSystemWrapper;

        public UserSystemFileProvider(ITemporaryFileProvider tmpFileProvider, FileSystemWrapper fileSystemWrapper)
        {
            _tmpFileProvider = tmpFileProvider;
        }

        public override async Task<string> SaveAsync(Stream stream, string fileName, string contentType)
        {
            string filePath;
            using (var fileStream = _tmpFileProvider.CreateTemporaryFile(Path.GetExtension(fileName), out filePath))
            {
                await stream.CopyToAsync(fileStream);
            }

            return filePath;
        }

        public override Task<FileResult> Get(string id)
        {
            var path = _tmpFileProvider.ResolveTemporaryPath(id);
            return Task.FromResult(new FileResult(System.IO.File.OpenRead(path), Path.GetFileName(path)));
        }

        public override Task DeleteAsync(string id)
        {
            var path = _tmpFileProvider.ResolveTemporaryPath(id);
            _fileSystemWrapper.DeleteFile(path);

            return Task.FromResult(0);
        }
    }
}