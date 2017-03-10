using System;
using System.Threading;
using CVaS.AlgServer.Services.BrokerReceiver;
using CVaS.AlgServer.Services.MessageProcessor;
using Microsoft.Extensions.Logging;

namespace CVaS.AlgServer.Services.Server
{
    public class BrokerServer
    {
        private readonly IBrokerReceiver _brokerReceiver;
        private readonly ILogger<BrokerServer> _logger;
        private readonly IMessageProcessor _messageProcessor;

        public BrokerServer(IBrokerReceiver brokerReceiver, ILogger<BrokerServer> logger, IMessageProcessor messageProcessor)
        {
            _brokerReceiver = brokerReceiver;
            _logger = logger;
            _messageProcessor = messageProcessor;
        }

        public void Start()
        {
            var exitEvent = new ManualResetEvent(false);
            Console.CancelKeyPress += (sender, eventArgs) => {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };

            try
            {
                _brokerReceiver.Setup(_messageProcessor);

                exitEvent.WaitOne();
            }
            catch (ArgumentException)
            {
                exitEvent.Set();
                _logger.LogError("Coudln't connect to RabbitMq server: Disconnected bus");
            }
            catch (TimeoutException)
            {
                exitEvent.Set();
                _logger.LogError("Coudln't connect to RabbitMq server: Timeout");
            }
        }
    }
}