using System.Threading.Tasks;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Messages;
using CVaS.Shared.Repositories;
using CVaS.Shared.Services.Launch;
using LightInject;
using Microsoft.Extensions.Logging;

namespace CVaS.AlgServer.Services
{
    internal class RunMessageProcessor : IMessageProcessor
    {
        private readonly ILogger<RunMessageProcessor> _logger;
        private readonly ILaunchService _launchService;
        private readonly RunRepository _runRepository;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IServiceContainer _serviceContainer;

        public RunMessageProcessor(ILogger<RunMessageProcessor> logger, ILaunchService launchService, RunRepository runRepository,
            IUnitOfWorkProvider unitOfWorkProvider, IServiceContainer serviceContainer)
        {
            _logger = logger;
            _launchService = launchService;
            _runRepository = runRepository;
            _unitOfWorkProvider = unitOfWorkProvider;
            _serviceContainer = serviceContainer;
        }
        public async Task<AlgorithmResultMessage> ProcessAsync(CreateAlgorithmMessage request)
        {
            _logger.LogInformation("Processing message - Run Id: " + request.RunId);

            using (_serviceContainer.BeginScope())
            {

                using (_unitOfWorkProvider.Create())
                {
                    var run = await _runRepository.GetById(request.RunId);

                    var result = await _launchService.LaunchAsync(request.FilePath, request.Arguments, run);

                    return new AlgorithmResultMessage()
                    {
                        StdOut = result.StdOut,
                        StdErr = result.StdErr,
                        Duration = result.Duration,
                        FileName = result.FileName,
                        Result = result.Result,
                        RunId = result.RunId
                    };
                }

            }
        }
    }
}