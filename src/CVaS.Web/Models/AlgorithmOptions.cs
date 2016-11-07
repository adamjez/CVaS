using System.Collections.Generic;
using CVaS.DAL.Model;

namespace CVaS.Web.Models
{
    public class AlgorithmOptions
    {
        public IList<AlgorithmArgument> Arguments { get; set; }
    }

    public class AlgorithmArgument
    {
        public ArgumentType Type { get; set; }
        public string Content { get; set; }
    }
}
