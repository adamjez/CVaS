using System.IO;
using System.Threading.Tasks;

namespace CVaS.Shared.Services.File.Providers
{
    public class SystemFileProvider : FileProvider
    {
        private readonly TemporaryFileProvider _tmpFileProvider;
        public SystemFileProvider(TemporaryFileProvider tmpFileProvider)
        {
            _tmpFileProvider = tmpFileProvider;
        }

        public override async Task<string> Save(Stream stream, string fileName, string contentType)
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
            var path = _tmpFileProvider.ResolveTemporaryFilePath(id);
            return Task.FromResult(new FileResult(System.IO.File.OpenRead(path), Path.GetFileName(path)));
        }
    }
}