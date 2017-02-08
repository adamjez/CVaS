using System;
using System.Threading;
using CVaS.Shared.Messages;
using Microsoft.Extensions.Logging;

namespace CVaS.AlgServer.Services
{
    public class Core
    {
        private readonly IBrokerReceiver _brokerReceiver;
        private readonly ILogger<Core> _logger;

        public Core(IBrokerReceiver brokerReceiver, ILogger<Core> logger)
        {
            _brokerReceiver = brokerReceiver;
            _logger = logger;
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
                _brokerReceiver.Setup(ProcessingMessage);

                exitEvent.WaitOne();
            }
            catch (System.TimeoutException)
            {
                _logger.LogError("Coudln't connect to RabbitMq server");
            }
        }

        private AlgorithmResultMessage ProcessingMessage(CreateAlgorithmMessage request)
        {
            _logger.LogInformation("Processing message - Id: " + request.AlgorithmId);

            return new AlgorithmResultMessage() {StdOut = "Hello World!"};
        }
    }
}