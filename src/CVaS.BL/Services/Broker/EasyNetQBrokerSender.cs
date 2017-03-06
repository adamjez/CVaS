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

        public async Task<RunResultMessage> SendAsync(CreateAlgorithmMessage message)
        {
            return await _bus.RequestAsync<CreateAlgorithmMessage, RunResultMessage>(message);
        }

        public async Task<RunResultMessage> SendAsync(CreateAlgorithmMessage message, TimeSpan timeout)
        {
            var result = await SendAsync(message).WithTimeout(timeout);

            if (result.Completed)
            {
                throw new TimeoutException();
            }

            return result.Value;
        }
    }
}