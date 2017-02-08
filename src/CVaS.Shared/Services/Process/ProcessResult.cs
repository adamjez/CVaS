using System;

namespace CVaS.Shared.Services.Process
{
    public class ProcessResult
    {
        public string StdOut { get; set; }
        public string StdError { get; set; }
        public int ExitCode { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime FinishedAt { get; set; }
    }
}