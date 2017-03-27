using System.Threading.Tasks;
using CVaS.Shared.Options;
using EasyNetQ.Management.Client;
using System.Linq;
using EasyNetQ;
using Microsoft.Extensions.Options;
using System;
using Microsoft.Extensions.Logging;

namespace CVaS.Shared.Services.Broker
{
    public class BrokerStatus : IBrokerStatus
    {
        private readonly IOptions<BrokerOptions> _brokerOptions;
        private readonly IBus _bus;
        private readonly ILogger<BrokerStatus> _logger;

        public BrokerStatus(IOptions<BrokerOptions> brokerOptions, IBus bus, ILogger<BrokerStatus> logger)
        {
            _logger = logger;
            _bus = bus;
            _brokerOptions = brokerOptions;
        }

        public Task<int?> GetConnectedAlgServersCount()
        {
            if (!_bus.IsConnected)
            {
                return null;
            }

            var client = new ManagementClient(_brokerOptions.Value.Hostname, _brokerOptions.Value.Username, _brokerOptions.Value.Password);

            try
            {
                return Task.FromResult<int?>(
                    client.GetConnections()
                        .Where(c => c.ClientProperties.Application == "CVaS.AlgServer.dll")
                        .Count());

            }
            catch (Exception exc)
            {
                _logger.LogError("Exception when getting connected Alg Servers Count", exc);
            }

            return null;
        }

        public string GetStatus()
        {
            return _bus.IsConnected ? "Connected" : "Disconnected";
        }
    }
}