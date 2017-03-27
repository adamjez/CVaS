using System.IO;
using System.Threading.Tasks;

namespace CVaS.Shared.Services.File.User
{
    public abstract class UserFileProvider : IUserFileProvider
    {
        public async Task<string> SaveAsync(string filePath, string contentType)
        {
            using (var stream = System.IO.File.OpenRead(filePath))
                return await SaveAsync(stream, Path.GetFileName(filePath), contentType);
        }

        public abstract Task<string> SaveAsync(Stream stream, string fileName, string contentType);
        public abstract Task<FileResult> GetAsync(string id);
        public abstract Task DeleteAsync(string id);
    }
}