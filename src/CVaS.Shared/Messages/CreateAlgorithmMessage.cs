using System.Collections.Generic;
using CVaS.Shared.Services.Argument;

namespace CVaS.Shared.Messages
{
    public class CreateAlgorithmMessage
    {
        public int AlgorithmId { get; set; }
        public int RunId { get; set; }
        public List<Argument> Arguments { get; set; }
    }
}