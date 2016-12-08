using CVaS.DAL.Model;

namespace CVaS.Web.Models
{
    public class AlgorithmResult
    {
        public string Title { get; set; }
        public string StdOut { get; set; }
        public string StdError { get; set; }
        public string Zip { get; set; }
        public int RunId { get; set; }
        public RunResultType Result { get; set; }
    }
}
