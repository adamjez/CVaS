using System.Collections.Generic;
using System.Threading.Tasks;

namespace CVaS.Shared.Services.File.Algorithm
{
    public interface IAlgorithmFileProvider
    {
        string GetAlgorithmFilePath(string codeName, string algFile);
        Task DownloadFiles(List<Argument.Argument> arguments, int userId);
    }
}