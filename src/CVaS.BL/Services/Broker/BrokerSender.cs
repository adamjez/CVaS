using System;
using System.Text;
using CVaS.AlgServer.Options;
using CVaS.Shared.Messages;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CVaS.BL.Services.Broker
{
    public class BrokerSender : IBrokeSender
    {
        private readonly IOptions<BrokerOptions> _brokerOptions;
        private IConnection _connection;
        private IModel _channel;

        public BrokerSender(IOptions<BrokerOptions> brokerOptions)
        {
            _brokerOptions = brokerOptions;

            var factory = new ConnectionFactory() { HostName = _brokerOptions.Value.Hostname };
            _connection = factory.CreateConnection();

            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: _brokerOptions.Value.Queue,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
        }

        public AlgorithmResultMessage Send(CreateAlgorithmMessage msg)
        {
            var message = "Hello World";
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();

            _channel.BasicPublish(exchange: "",
                                    routingKey: _brokerOptions.Value.Queue,
                                    basicProperties: properties,
                                    body: body);


            Console.WriteLine(" [x] Sent {0}", message);
            return null;
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _channel = null;

            _connection?.Dispose();
            _connection = null;
        }
    }
}