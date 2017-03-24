using CVaS.DAL.Model;
using System;

namespace CVaS.Shared.Models
{
    public class RunResult
    {
        public Guid? FileId { get; set; }
        public string StdOut { get; set; }
        public string StdErr { get; set; }
        public Guid RunId { get; set; }
        public RunResultType Result { get; set; }
        public double? Duration { get; set; }
    }
}