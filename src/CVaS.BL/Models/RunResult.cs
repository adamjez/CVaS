using CVaS.DAL.Model;

namespace CVaS.BL.Models
{
    public class RunResult
    {
        public string FileName { get; set; }
        public string StdOut { get; set; }
        public string StdErr { get; set; }
        public int RunId { get; set; }
        public RunResultType Result { get; set; }
        public double Duration { get; set; }
    }
}