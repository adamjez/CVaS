using System.Collections;
using System.Collections.Generic;

namespace CVaS.Web.Models
{
    public class AlgorithmOptions
    {
        //public string Arguments { get; set; }
        public IList<AlgorithmArgument> Arguments { get; set; }
    }

    public class AlgorithmArgument
    {
        public ArgumentType Type { get; set; }
        public string Content { get; set; }
    }
}
