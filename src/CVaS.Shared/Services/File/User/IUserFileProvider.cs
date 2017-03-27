using System.IO;
using System.Threading.Tasks;

namespace CVaS.Shared.Services.File.User
{
    public interface IUserFileProvider
    {
        Task<string> SaveAsync(string filePath, string contentType);

        Task<string> SaveAsync(Stream stream, string fileName, string contentType);

        Task<FileResult> GetAsync(string id);

        Task DeleteAsync(string id);
    }
}