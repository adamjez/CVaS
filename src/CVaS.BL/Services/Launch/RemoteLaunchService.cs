using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CVaS.BL.Services.Broker;
using CVaS.DAL.Model;
using CVaS.Shared.Exceptions;
using CVaS.Shared.Messages;
using CVaS.Shared.Models;
using CVaS.Shared.Services.Argument;
using CVaS.Shared.Services.Launch;
using Microsoft.Extensions.Logging;

namespace CVaS.BL.Services.Launch
{
    public class RemoteLaunchService : ILaunchService
    {
        private readonly IBrokerSender _brokerSender;
        private readonly ILogger<RemoteLaunchService> _logger;

        public RemoteLaunchService(IBrokerSender brokerSender, ILogger<RemoteLaunchService> logger)
        {
            _brokerSender = brokerSender;
            _logger = logger;
        }

        public async Task<RunResult> LaunchAsync(string codeName, string filePath, List<Argument> args, Run run)
        {
            var message = new CreateAlgorithmMessage()
            {
                Arguments = args,
                RunId = run.Id
            };

            try
            {
                var result = await _brokerSender.SendAsync(message);
                if (result?.Exception != null)
                {
                    var innerExc = (ApiException)Activator.CreateInstance(result.Exception);
                    throw new RemoteException(innerExc);
                }
                return result;
            }
            catch (TimeoutException)
            {
                _logger.LogError("Timeouted when sending message to broker (Alg Server is failing or not any is connected)");
            }
            
            return new RunResult()
            {
                Result = RunResultType.NotFinished,
                RunId = run.Id
            };
        }
    }
}