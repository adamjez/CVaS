using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CVaS.DAL.Model;
using CVaS.BL.DTO;
using CVaS.BL.Services.ArgumentTranslator;
using CVaS.Shared.Core;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Exceptions;
using CVaS.Shared.Models;
using CVaS.Shared.Providers;
using CVaS.Shared.Repositories;
using CVaS.Shared.Services.Argument;
using CVaS.Shared.Services.Launch;

namespace CVaS.BL.Facades
{
    public class RunFacade : AppFacadeBase
    {
        private readonly AlgorithmRepository _algorithmRepository;
        private readonly RunRepository _runRepository;

        private readonly IArgumentTranslator _argumentTranslator;
        private readonly ILaunchService _launchService;

        public RunFacade(IUnitOfWorkProvider unitOfWorkProvider, ICurrentUserProvider currentUserProvider,
            AlgorithmRepository algorithmRepository, RunRepository runRepository, IArgumentTranslator argumentTranslator,
            ILaunchService launchService)
            : base(unitOfWorkProvider, currentUserProvider)
        {
            _algorithmRepository = algorithmRepository;
            _runRepository = runRepository;
            _argumentTranslator = argumentTranslator;
            _launchService = launchService;
        }

        public async Task<RunResult> RunAlgorithmAsync(string codeName, IEnumerable<object> arguments)
        {
            using (UnitOfWorkProvider.Create())
            {
                var algorithm = await _algorithmRepository.GetByCodeNameWithArgs(codeName);

                if (algorithm == null)
                {
                    throw new NotFoundException("Given algorithm codeName doesn't exists");
                }

                var args = _argumentTranslator.Process(arguments);

                // Create Run In Db
                return await CreateRunAndLaunch(algorithm, args);
            }
        }


        public async Task<RunResult> RunHelpAsync(string codeName)
        {
            return await RunAlgorithmAsync(codeName, new List<object>() {"--help"});
        }

        private async Task<RunResult> CreateRunAndLaunch(Algorithm algorithm, List<Argument> args)
        {
            using (var uow = UnitOfWorkProvider.Create(DbContextOptions.DisableTransactionMode))
            {
                var run = new Run()
                {
                    AlgorithmId = algorithm.Id,
                    UserId = CurrentUserProvider.Id,
                    Result = RunResultType.NotFinished
                };

                _runRepository.Insert(run);
                await uow.CommitAsync();

                return await _launchService.LaunchAsync(algorithm.CodeName, algorithm.FilePath, args, run);
            }
        }

        public async Task<RunDTO> GetSafelyAsync(int runId)
        {
            using (UnitOfWorkProvider.Create())
            {
                var run = await _runRepository.GetByIdSafely(runId, r => r.Algorithm);

                if (run.UserId != CurrentUserProvider.Id)
                {
                    throw new UnauthorizedAccessException();
                }

                return new RunDTO()
                {
                    Id = run.Id,
                    CreatedAt = run.CreatedAt,
                    FinishedAt = run.FinishedAt,

                    FileId = run.FileId,
                    Result = run.Result,
                    StdOut = run.StdOut,
                    StdErr = run.StdErr,
                    AlgorithmCode = run.Algorithm.CodeName
                };
            }
        }
    }
}