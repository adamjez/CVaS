using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CVaS.BL.Services.Process
{
    public interface IProcessService
    {
        Task<ProcessResult> RunAsync(string filePath, IList<string> arguments, CancellationToken cancellationToken);
    }

    public class ProcessResult
    {
        public string StdOut { get; set; }
        public string StdError { get; set; }
        public int ExitCode { get; set; }
    }
}
