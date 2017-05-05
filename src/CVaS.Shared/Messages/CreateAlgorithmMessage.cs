using System.Collections.Generic;
using CVaS.Shared.Services.Argument;
using System;
using CVaS.DAL.Model;

namespace CVaS.Shared.Messages
{
    public class CreateAlgorithmMessage
    {
        public Algorithm Algorithm { get; set; }
        public Run Run { get; set; }
        public List<Argument> Arguments { get; set; }
        public int? Timeout { get; set; }
    }
}