using System.Collections.Generic;
using CVaS.Shared.Services.Argument;
using System;

namespace CVaS.Shared.Messages
{
    public class CreateAlgorithmMessage
    {
        public int AlgorithmId { get; set; }
        public Guid RunId { get; set; }
        public List<Argument> Arguments { get; set; }
        public int? Timeout { get; set; }
    }
}