using System;
using System.Text;
using CVaS.AlgServer.Options;
using CVaS.Shared.Messages;
using CVaS.Shared.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CVaS.AlgServer.Services
{
    public class RabbitMqReceiver : IBrokerReceiver
    {
        private const string QueueName = "algBasicQueue";
        private readonly IOptions<BrokerOptions> _brokerOptions;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMqReceiver(IOptions<BrokerOptions> brokerOptions)
        {
            _brokerOptions = brokerOptions;
        }

        public void Setup(IMessageProcessor messagesProcessor)
        {
            var factory = new ConnectionFactory() { HostName = _brokerOptions.Value.Hostname };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: QueueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            //_channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);

                // ToDo add deserializing
                messagesProcessor.ProcessAsync(new CreateAlgorithmMessage());
                Console.WriteLine(" [x] Received {0}", message);

                var eventRecieved = model as EventingBasicConsumer;
                eventRecieved?.Model?.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue: QueueName, noAck: false, consumer: consumer);

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