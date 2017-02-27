using System.Threading.Tasks;
using CVaS.Shared.Messages;
using EasyNetQ;

namespace CVaS.BL.Services.Broker
{
    public class EasyNetQBrokerSender : IBrokerSender
    {
        private readonly IBus _bus;

        public EasyNetQBrokerSender(IBus bus)
        {
            _bus = bus;
        }

        public async Task<AlgorithmResultMessage> SendAsync(CreateAlgorithmMessage message)
        {
            return await _bus.RequestAsync<CreateAlgorithmMessage, AlgorithmResultMessage>(message);
        }
    }
}