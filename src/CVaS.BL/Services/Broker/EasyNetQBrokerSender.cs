using CVaS.Shared.Messages;
using CVaS.Shared.Options;
using EasyNetQ;
using Microsoft.Extensions.Options;

namespace CVaS.BL.Services.Broker
{
    public class EasyNetQBrokerSender : IBrokeSender
    {
        private readonly IBus _bus;

        public EasyNetQBrokerSender(IOptions<BrokerOptions> brokerOptions, IBus bus)
        {
            _bus = bus;
        }

        public AlgorithmResultMessage Send(CreateAlgorithmMessage message)
        {
            return _bus.Request<CreateAlgorithmMessage, AlgorithmResultMessage>(message);
        }
    }
}