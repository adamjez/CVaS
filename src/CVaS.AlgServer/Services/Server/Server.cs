using System;
using System.Threading;
using CVaS.AlgServer.Services.BrokerReceiver;
using CVaS.AlgServer.Services.MessageProcessor;
using Microsoft.Extensions.Logging;

namespace CVaS.AlgServer.Services.Server
{
    public class Server
    {
        private readonly IBrokerReceiver _brokerReceiver;
        private readonly ILogger<Server> _logger;
        private readonly IMessageProcessor _messageProcessor;

        public Server(IBrokerReceiver brokerReceiver, ILogger<Server> logger, IMessageProcessor messageProcessor)
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
            catch (System.TimeoutException)
            {
                _logger.LogError("Coudln't connect to RabbitMq server");
            }
        }
    }
}