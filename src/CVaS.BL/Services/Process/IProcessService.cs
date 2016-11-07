using System.Collections.Generic;

namespace CVaS.BL.Services.Process
{
    public interface IProcessService
    {
        ProcessResult Run(string filePath, string workingDirectory, IList<string> arguments);
    }

    public class ProcessResult
    {
        public string StdOut { get; set; }
        public string StdError { get; set; }
    }
}
