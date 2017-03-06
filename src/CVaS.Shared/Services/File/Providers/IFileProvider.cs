using System.IO;
using System.Threading.Tasks;

namespace CVaS.Shared.Services.File.Providers
{
    public interface IFileProvider
    {
        Task<string> Save(string filePath, string contentType);

        Task<string> Save(Stream stream, string fileName, string contentType);

        Task<FileResult> Get(string id);
    }
}