using CVaS.AlgServer.Services.MessageProcessor;

namespace CVaS.AlgServer.Services.BrokerReceiver
{
    public interface IBrokerReceiver
    {
        void Setup(IMessageProcessor messagesProcessor);
    }
}