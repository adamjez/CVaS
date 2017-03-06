using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CVaS.DAL.Model;
using CVaS.BL.DTO;
using CVaS.BL.Services.ArgumentTranslator;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Exceptions;
using CVaS.Shared.Models;
using CVaS.Shared.Providers;
using CVaS.Shared.Repositories;
using CVaS.Shared.Services.File;
using CVaS.Shared.Services.Launch;
using CVaS.Shared.Services.Process;

namespace CVaS.BL.Facades
{
    public class RunFacade : AppFacadeBase
    {
        private readonly AlgorithmRepository _algorithmRepository;
        private readonly RunRepository _runRepository;

        private readonly FileHelper _fileHelper;
        private readonly AlgorithmFileProvider _algFileProvider;

        private readonly IProcessService _processService;
        private readonly IArgumentTranslator _argumentTranslator;
        private readonly ILaunchService _launchService;

        public RunFacade(IUnitOfWorkProvider unitOfWorkProvider, ICurrentUserProvider currentUserProvider,
            AlgorithmRepository algorithmRepository, IProcessService processService, FileHelper fileHelper, 
            AlgorithmFileProvider algFileProvider, RunRepository runRepository, IArgumentTranslator argumentTranslator, 
            ILaunchService launchService)
            : base(unitOfWorkProvider, currentUserProvider)
        {
            _algorithmRepository = algorithmRepository;
            _processService = processService;
            _fileHelper = fileHelper;
            _algFileProvider = algFileProvider;
            _runRepository = runRepository;
            _argumentTranslator = argumentTranslator;
            _launchService = launchService;
        }

        public async Task<RunResult> RunProcessAsync(string codeName, IEnumerable<object> arguments)
        {
            using (var uow = UnitOfWorkProvider.Create())
            {
                var algorithm = await _algorithmRepository.GetByCodeNameWithArgs(codeName);

                if (algorithm == null)
                {
                    throw new NotFoundException("Given algorithm codeName doesn't exists");
                }

                var args = _argumentTranslator.Process(arguments);

                // Create Run In Db
                var run = new Run()
                {
                    AlgorithmId = algorithm.Id,
                    UserId = CurrentUserProvider.Id,
                    Result = RunResultType.NotFinished
                };

                _runRepository.Insert(run);
                await uow.CommitAsync();

                return await _launchService.LaunchAsync(codeName, algorithm.FilePath, args, run);
            }
        }

        public async Task<ProcessResult> RunHelpAsync(string codeName)
        {
            using (UnitOfWorkProvider.Create())
            {
                var algorithm = await _algorithmRepository.GetByCodeNameWithArgs(codeName);

                if (algorithm == null)
                {
                    throw new NotFoundException("Given algorithm codeName doesn't exists");
                }

                var filePath = _algFileProvider.GetAlgorithmFilePath(codeName, algorithm.FilePath);

                if (!_fileHelper.Exists(filePath))
                {
                    throw new NotFoundException("Given algorithm execution file doesn't exists");
                }

                var arguments = new List<string>() {"--help"};
                return await _processService.RunAsync(filePath, arguments, CancellationToken.None);
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