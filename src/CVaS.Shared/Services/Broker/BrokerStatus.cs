using System.Threading.Tasks;
using CVaS.Shared.Options;
using EasyNetQ.Management.Client;
using System.Linq;
using EasyNetQ;
using Microsoft.Extensions.Options;

namespace CVaS.Shared.Services.Broker
{
    public class BrokerStatus
    {
        private readonly IOptions<BrokerOptions> _brokerOptions;
        private readonly IBus _bus;

        public BrokerStatus(IOptions<BrokerOptions> brokerOptions, IBus bus)
        {
            _bus = bus;
            _brokerOptions = brokerOptions;
        }

        public Task<int?> GetConnectedClients()
        {
            if (!_bus.IsConnected)
            {
                return null;
            }

            var client = new ManagementClient("https://"+_brokerOptions.Value.Hostname, _brokerOptions.Value.Username, _brokerOptions.Value.Password, 443);

            return Task.FromResult<int?>(client.GetConnections().Count());
        }
    }
}