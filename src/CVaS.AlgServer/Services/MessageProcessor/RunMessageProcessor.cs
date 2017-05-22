using System;
using System.Threading.Tasks;
using CVaS.Shared.Core;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Exceptions;
using CVaS.Shared.Messages;
using CVaS.Shared.Repositories;
using CVaS.Shared.Services.Launch;
using Microsoft.Extensions.Logging;
using DryIoc;
using CVaS.Shared.Models;

namespace CVaS.AlgServer.Services.MessageProcessor
{
    internal class RunMessageProcessor : IMessageProcessor
    {
        private readonly ILogger<RunMessageProcessor> _logger;
        private readonly ILaunchService _launchService;
        private readonly RunRepository _runRepository;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IContainer _container;

        public RunMessageProcessor(ILogger<RunMessageProcessor> logger, ILaunchService launchService, RunRepository runRepository,
            IUnitOfWorkProvider unitOfWorkProvider, IContainer container)
        {
            _logger = logger;
            _launchService = launchService;
            _runRepository = runRepository;
            _unitOfWorkProvider = unitOfWorkProvider;
            _container = container;
        }
        public async Task<RunResultMessage> ProcessAsync(CreateAlgorithmMessage request)
        {
            try
            {
                return await ProcessInternalAsync(request);
            }
            catch (ApiException ex)
            {
                return new RunResultMessage()
                {
                    Exception = ex.GetType()
                };
            }
            catch (UnauthorizedAccessException)
            {
                return new RunResultMessage()
                {
                    Exception = typeof(UnauthorizedAccessException)
                };
            }
        }

        private async Task<RunResultMessage> ProcessInternalAsync(CreateAlgorithmMessage request)
        {
            _logger.LogInformation("Processing message - Run Id: " + request.Run.Id);

            using (_container.OpenScope())
            {
                using (var uow = _unitOfWorkProvider.Create(DbContextOptions.DisableTransactionMode))
                {
                    uow.Context.Run.Attach(request.Run);

                    var result = await _launchService.LaunchAsync(request.Algorithm, request.Run, 
                        new RunSettings { Arguments = request.Arguments, Timeout = request.Timeout });

                    return new RunResultMessage()
                    {
                        StdOut = result.StdOut,
                        StdErr = result.StdErr,
                        CreatedAt = result.CreatedAt,
                        FinishedAt = result.FinishedAt,
                        FileId = result.FileId,
                        Result = result.Result,
                        RunId = result.RunId
                    };
                }
            }
        }
    }
}