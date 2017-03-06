using System;
using CVaS.Shared.Models;

namespace CVaS.Shared.Messages
{
    public class RunResultMessage : RunResult
    {
        public Type Exception { get; set; }
    }
}