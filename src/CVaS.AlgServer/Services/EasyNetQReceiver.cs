using System;
using CVaS.AlgServer.Options;
using CVaS.Shared.Messages;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CVaS.AlgServer.Services
{
    public class EasyNetQReceiver : IBrokerReceiver
    {
        private readonly IOptions<BrokerOptions> _brokerOptions;
        private readonly IBus _bus;
        private readonly ILogger<EasyNetQReceiver> _logger;

        public EasyNetQReceiver(IOptions<BrokerOptions> brokerOptions, IBus bus, ILogger<EasyNetQReceiver> logger)
        {
            _brokerOptions = brokerOptions;
            _bus = bus;
            _logger = logger;
        }

        public void Setup(Func<CreateAlgorithmMessage, AlgorithmResultMessage> messageProcessingFunc)
        {
            if (!_bus.IsConnected)
            {
                _logger.LogDebug("Trying to communicate with bus that is not connected to RabbitMq server!");
            }
            else
            {
                _logger.LogInformation("Connected to RabbitMq server and setting respond");
            }
            
            _bus.Respond<CreateAlgorithmMessage, AlgorithmResultMessage>((request) =>
            {
                _logger.LogInformation("Received request!");
                var result =  messageProcessingFunc(request);
                _logger.LogInformation("Sending respond!");
                return result;
            });
        }
    }
}