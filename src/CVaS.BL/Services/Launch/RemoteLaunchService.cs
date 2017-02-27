using System.Collections.Generic;
using System.Threading.Tasks;
using CVaS.BL.Services.Broker;
using CVaS.DAL.Model;
using CVaS.Shared.Messages;
using CVaS.Shared.Models;
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

        public async Task<RunResult> LaunchAsync(string filePath, List<string> args, Run run)
        {
            var message = new CreateAlgorithmMessage()
            {
                Arguments = args,
                FilePath = filePath,
                RunId = run.Id
            };

            try
            {
                return await _brokerSender.SendAsync(message);
            }
            catch(System.Exception exc)
            {
                _logger.LogCritical(exc.ToString());
            }

            return new RunResult()
            {
                Result = RunResultType.NotFinished
            };
        }
    }
}