using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CVaS.Shared.Services.Process
{
    public interface IProcessService
    {
        Task<ProcessResult> RunAsync(string filePath, IList<string> arguments, string workingDirectory, CancellationToken cancellationToken);
        Task<ProcessResult> RunAsync(string filePath, IList<string> arguments, CancellationToken cancellationToken);
    }
}
