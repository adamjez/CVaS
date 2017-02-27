﻿using System;
using System.Text;
using System.Threading.Tasks;
using CVaS.Shared.Messages;
using CVaS.Shared.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CVaS.BL.Services.Broker
{
    public class BrokerSender : IBrokerSender
    {
        private const string QueueName = "algBasicQueue";

        private readonly IOptions<BrokerOptions> _brokerOptions;
        private IConnection _connection;
        private IModel _channel;

        public BrokerSender(IOptions<BrokerOptions> brokerOptions)
        {
            _brokerOptions = brokerOptions;

            var factory = new ConnectionFactory() { HostName = _brokerOptions.Value.Hostname };
            _connection = factory.CreateConnection();

            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: QueueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
        }

        public Task<AlgorithmResultMessage> SendAsync(CreateAlgorithmMessage msg)
        {
            var message = "Hello World";
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();

            _channel.BasicPublish(exchange: "",
                                    routingKey: QueueName,
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