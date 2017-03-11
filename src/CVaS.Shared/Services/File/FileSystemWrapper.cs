using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CVaS.Shared.Services.File
{
    public class FileSystemWrapper
    {
        public async Task SaveAsync(Stream stream, string filePath)
        {
            using (stream)
            using (var file = System.IO.File.Create(filePath))
                await stream.CopyToAsync(file);
        }

        public void CreateDirectory(string directoryPath)
        {
            System.IO.Directory.CreateDirectory(directoryPath);
        }

        public void DeleteDirectory(string directoryPath)
        {
            System.IO.Directory.Delete(directoryPath, true);
        }

        public void DeleteFile(string filePath)
        {
            System.IO.File.Delete(filePath);
        }

        public bool ExistsFile(string path)
        {
            return System.IO.File.Exists(path);
        }

        public bool ExistsDirectory(string path)
        {
            return System.IO.Directory.Exists(path);
        }

        public bool IsEmpty(string directory)
        {
            return !Directory.EnumerateFiles(directory).Any();
        }

        public string GetDirectoryFromFile(string filePath)
        {
            return Directory.GetParent(filePath).FullName;
        }
    }
}