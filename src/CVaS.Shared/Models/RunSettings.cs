using System.Collections.Generic;
using CVaS.Shared.Services.Argument;

namespace CVaS.Shared.Models
{
    public class RunSettings
    {
        public List<Argument> Arguments { get; set; }
        public int? Timeout { get; set; }
    }
}