using System;
using CVaS.Shared.Messages;

namespace CVaS.AlgServer.Services.BrokerReceiver
{
    public class TaskReceivedEventArgs : EventArgs
    {
        public CreateAlgorithmMessage Message { get; set; }
    }
}