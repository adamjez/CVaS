using System.IO;

namespace CVaS.Shared.Services.File.Temporary
{
    public interface ITemporaryFileProvider
    {
        string CreateTemporaryFolder();

        string ResolveTemporaryPath();

        string ResolveTemporaryPath(string file);

        FileStream CreateTemporaryFile(string extension, out string filePath);

        BasicFileInfo CreateTemporaryFilePath(string extesion = "");
    }
}