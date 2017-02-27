using System;
using System.Threading.Tasks;
using CVaS.Shared.Messages;

namespace CVaS.BL.Services.Broker
{
    public interface IBrokerSender
    {
        Task<AlgorithmResultMessage> SendAsync(CreateAlgorithmMessage message);
        Task<AlgorithmResultMessage> SendAsync(CreateAlgorithmMessage message, TimeSpan timeout);

    }
}