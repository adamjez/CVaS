using System.IO;
using System.Threading.Tasks;
using CVaS.Shared.Options;
using CVaS.Shared.Services.File.Temporary;
using Microsoft.Extensions.Options;

namespace CVaS.Shared.Services.File.User
{
    public class FileSystemStorage : FileStorageBase
    {
        private readonly string _localPath;
        private readonly FileSystemWrapper _fileSystemWrapper;

        public FileSystemStorage(IOptions<DirectoryPathOptions> directoryOptions, FileSystemWrapper fileSystemWrapper)
        {
            _localPath = directoryOptions.Value.Storage;
            _fileSystemWrapper = fileSystemWrapper;
        }

        public override async Task<string> SaveAsync(Stream stream, string fileName, string contentType)
        {
            var randomFileName = Path.GetRandomFileName() + Path.GetExtension(fileName);
            var id = ResolvePath(randomFileName);

            using (var fileStream = System.IO.File.OpenWrite(id))
            {
                await stream.CopyToAsync(fileStream);
            }

            return id;
        }

        public override Task<FileResult> GetAsync(string id)
        {
            var path = ResolvePath(id);
            // Todo: resolve content type properly
            return Task.FromResult(new FileResult(System.IO.File.OpenRead(path)));
        }

        public override Task DeleteAsync(string id)
        {
            var path = ResolvePath(id);
            _fileSystemWrapper.DeleteFile(path);

            return Task.FromResult(0);
        }

        public string ResolvePath(string id)
        {
            return System.IO.Path.Combine(_localPath, id);
        }
    }
}