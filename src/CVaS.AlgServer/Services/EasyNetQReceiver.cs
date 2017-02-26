﻿using CVaS.Shared.Messages;
using EasyNetQ;
using Microsoft.Extensions.Logging;

namespace CVaS.AlgServer.Services
{
    public class EasyNetQReceiver : IBrokerReceiver
    {
        private readonly IBus _bus;
        private readonly ILogger<EasyNetQReceiver> _logger;

        public EasyNetQReceiver(IBus bus, ILogger<EasyNetQReceiver> logger)
        {
            _bus = bus;
            _logger = logger;
        }

        public void Setup(IMessageProcessor messageProcessor)
        {
            if (!_bus.IsConnected)
            {
                _logger.LogDebug("Trying to communicate with bus that is not connected to RabbitMq server!");
            }
            else
            {
                _logger.LogInformation("Connected to RabbitMq server and setting respond");
            }
            
            _bus.RespondAsync<CreateAlgorithmMessage, AlgorithmResultMessage>(async (request) =>
            {
                _logger.LogInformation("Received request!");
                var result = await messageProcessor.ProcessAsync(request);
                _logger.LogInformation("Sending respond!");
                return result;
            });
        }
    }
}