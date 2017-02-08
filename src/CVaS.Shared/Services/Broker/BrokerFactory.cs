using CVaS.AlgServer.Options;
using EasyNetQ;
using Microsoft.Extensions.Options;

namespace CVaS.Shared.Services.Broker
{
    public class BrokerFactory
    {
        private readonly IOptions<BrokerOptions> _brokerOptions;

        public BrokerFactory(IOptions<BrokerOptions> brokerOptions)
        {
            _brokerOptions = brokerOptions;
        }

        public IBus Bus => RabbitHutch.CreateBus($"host={_brokerOptions.Value.Hostname}");
    }
}