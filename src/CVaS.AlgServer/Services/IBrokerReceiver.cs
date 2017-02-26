using System;
using CVaS.Shared.Messages;

namespace CVaS.AlgServer.Services
{
    public interface IBrokerReceiver
    {
        void Setup(IMessageProcessor messagesProcessor);
    }
}