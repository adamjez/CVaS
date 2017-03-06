using System.IO;
using System.Threading.Tasks;

namespace CVaS.Shared.Services.File.Providers
{
    public abstract class FileProvider : IFileProvider
    {
        public async Task<string> Save(string filePath, string contentType)
        {
            using (var stream = System.IO.File.OpenRead(filePath))
                return await Save(stream, Path.GetFileName(filePath), contentType);
        }

        public abstract Task<string> Save(Stream stream, string fileName, string contentType);
        public abstract Task<FileResult> Get(string id);
    }
}