using System;
using System.Threading.Tasks;
using CVaS.Shared.Messages;

namespace CVaS.BL.Services.Broker
{
    public interface IBrokerSender
    {
        Task<RunResultMessage> SendAsync(CreateAlgorithmMessage message);
        Task<RunResultMessage> SendAsync(CreateAlgorithmMessage message, TimeSpan timeout);

    }
}