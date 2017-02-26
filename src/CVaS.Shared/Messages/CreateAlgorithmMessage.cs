using System.Collections.Generic;

namespace CVaS.Shared.Messages
{
    public class CreateAlgorithmMessage
    {
        public int AlgorithmId { get; set; }
        public int RunId { get; set; }
        public string FilePath { get; set; }
        public List<string> Arguments { get; set; }
    }
}