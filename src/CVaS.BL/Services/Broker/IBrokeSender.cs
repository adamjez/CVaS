using CVaS.Shared.Messages;

namespace CVaS.BL.Services.Broker
{
    public interface IBrokeSender
    {
        AlgorithmResultMessage Send(CreateAlgorithmMessage message);
    }
}