using System;
using CVaS.Shared.Messages;

namespace CVaS.AlgServer.Services
{
    public class TaskReceivedEventArgs : EventArgs
    {
        public CreateAlgorithmMessage Message { get; set; }
    }
}