using System;
using CVaS.Shared.Messages;

namespace CVaS.AlgServer.Services
{
    public interface IBrokerReceiver
    {
        void Setup(Func<CreateAlgorithmMessage, AlgorithmResultMessage> messageProcessingFunc);
    }
}