using CVaS.Shared.Options;
using EasyNetQ;
using Microsoft.Extensions.Options;

namespace CVaS.Shared.Services.Broker
{
    public class BrokerFactory
    {
        private readonly IOptions<BrokerOptions> _brokerOptions;
        private readonly IOptions<AlgorithmOptions> _algorithmOptions;

        public BrokerFactory(IOptions<BrokerOptions> brokerOptions, IOptions<AlgorithmOptions> algorithmOptions)
        {
            _brokerOptions = brokerOptions;
            _algorithmOptions = algorithmOptions;
        }

        public IBus Bus => RabbitHutch.CreateBus(
            $"host={_brokerOptions.Value.Hostname};" +
            $"username={_brokerOptions.Value.Username ?? "guest"};" +
            $"password={_brokerOptions.Value.Password ?? "guest"};" +
            $"timeout={_algorithmOptions.Value.HardTimeout}");
    }
}