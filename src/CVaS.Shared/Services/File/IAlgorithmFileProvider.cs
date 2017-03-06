using System.Collections.Generic;
using System.Threading.Tasks;

namespace CVaS.Shared.Services.File
{
    public interface IAlgorithmFileProvider
    {
        Task DownloadFiles(List<Argument.Argument> arguments, int userId);
    }
}