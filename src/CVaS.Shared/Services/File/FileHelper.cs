using System.IO;
using System.Linq;

namespace CVaS.Shared.Services.File
{
    public class FileHelper
    {
        public bool Exists(string path)
        {
            return System.IO.File.Exists(path);
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