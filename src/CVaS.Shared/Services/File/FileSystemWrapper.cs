using System;
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
            Directory.CreateDirectory(directoryPath);
        }

        public void DeleteDirectory(string directoryPath)
        {
            Directory.Delete(directoryPath, true);
        }

        public void DeleteFile(string filePath)
        {
            System.IO.File.Delete(filePath);
        }

        public bool ExistsFile(string path)
        {
            return System.IO.File.Exists(path);
        }

        public long FileSize(string path)
        {
            return new FileInfo(path).Length;
        }

        public bool ExistsDirectory(string path)
        {
            return Directory.Exists(path);
        }

        public int FilesCountInFolder(string path)
        {
            return Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly).Length;
        }

        public string GetDirectoryFromFile(string filePath)
        {
            return Directory.GetParent(filePath).FullName;
        }

        internal void TouchFile(string filePath)
        {
            System.IO.File.SetLastWriteTimeUtc(filePath, DateTime.UtcNow);
        }
    }
}