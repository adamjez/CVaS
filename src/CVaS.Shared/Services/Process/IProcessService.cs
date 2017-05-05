using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CVaS.Shared.Services.Process
{
    public interface IProcessService
    {
        Task<ProcessResult> RunAsync(ProcessOptions options, CancellationToken cancellationToken);
    }
}
