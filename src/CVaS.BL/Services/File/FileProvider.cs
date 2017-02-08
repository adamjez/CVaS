using System.IO;
using System.Linq;

namespace CVaS.BL.Services.File
{
    public class FileProvider
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