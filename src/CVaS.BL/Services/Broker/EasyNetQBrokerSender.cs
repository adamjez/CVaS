using System;
using System.Threading.Tasks;
using CVaS.Shared.Messages;
using EasyNetQ;
using CVaS.Shared.Helpers;

namespace CVaS.BL.Services.Broker
{
    public class EasyNetQBrokerSender : IBrokerSender
    {
        private readonly IBus _bus;

        public EasyNetQBrokerSender(IBus bus)
        {
            _bus = bus;
        }

        public async Task<AlgorithmResultMessage> SendAsync(CreateAlgorithmMessage message)
        {
            return await _bus.RequestAsync<CreateAlgorithmMessage, AlgorithmResultMessage>(message);
        }

        public async Task<AlgorithmResultMessage> SendAsync(CreateAlgorithmMessage message, TimeSpan timeout)
        {
            var result = await SendAsync(message).WithTimeout(timeout);

            if (result.Timeouted)
            {
                throw new TimeoutException();
            }

            return result.Value;
        }
    }
}