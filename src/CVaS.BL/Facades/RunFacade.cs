using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using CVaS.BL.Core.Provider;
using CVaS.BL.Exceptions;
using CVaS.BL.Helpers;
using CVaS.BL.Models;
using CVaS.BL.Providers;
using CVaS.BL.Repositories;
using CVaS.BL.Services.ArgumentTranslator;
using CVaS.BL.Services.File;
using CVaS.BL.Services.Process;
using CVaS.DAL.Model;
using System.Linq;
using CVaS.BL.Core;
using CVaS.BL.DTO;

namespace CVaS.BL.Facades
{
    public class RunFacade : AppFacadeBase
    {
        private readonly AlgorithmRepository _algorithmRepository;
        private readonly RunRepository _runRepository;

        private readonly FileProvider _fileProvider;
        private readonly AlgorithmFileProvider _algFileProvider;
        private readonly TemporaryFileProvider _fileSystemProvider;

        private readonly IProcessService _processService;
        private readonly IArgumentTranslator _argumentTranslator;
        private readonly AlgorithmConfiguration _configuration;

        public RunFacade(IUnitOfWorkProvider unitOfWorkProvider, ICurrentUserProvider currentUserProvider,
            AlgorithmRepository algorithmRepository, IProcessService processService, FileProvider fileProvider, 
            AlgorithmFileProvider algFileProvider, TemporaryFileProvider fileSystemProvider,
            RunRepository runRepository, IArgumentTranslator argumentTranslator, AlgorithmConfiguration configuration)
            : base(unitOfWorkProvider, currentUserProvider)
        {
            _algorithmRepository = algorithmRepository;
            _processService = processService;
            _fileProvider = fileProvider;
            _algFileProvider = algFileProvider;
            _fileSystemProvider = fileSystemProvider;
            _runRepository = runRepository;
            _argumentTranslator = argumentTranslator;
            _configuration = configuration;
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

                var filePath = _algFileProvider.GetAlgorithmFilePath(codeName, algorithm.FilePath);

                if (!_fileProvider.Exists(filePath))
                {
                    throw new NotFoundException("Given algorithm execution file doesn't exists");
                }

                var args = await _argumentTranslator.ProcessAsync(arguments);

                var runFolder = _fileSystemProvider.CreateTemporaryFolder();

                // Create Run In Db
                var run = new Run()
                {
                    AlgorithmId = algorithm.Id,
                    UserId = CurrentUserProvider.Id,
                    Result = RunResultType.NotFinished
                };

                _runRepository.Insert(run);
                await uow.CommitAsync();

                args.Insert(0, runFolder);

                var tokenSource = new CancellationTokenSource(_configuration.HardTimeout * 1000);

                var task = _processService.RunAsync(filePath, args, tokenSource.Token);

                var lightTokenSource = new CancellationTokenSource();
                var firstTimeOutedTask = await Task.WhenAny(task, Task.Delay(_configuration.LightTimeout * 1000, lightTokenSource.Token));

                ProcessResult result = null;
                if (firstTimeOutedTask == task)
                {
                    lightTokenSource.Cancel();
                    // Task completed within timeout.
                    // Consider that the task may have faulted or been canceled.
                    // We re-await the task so that any exceptions/cancellation is rethrown.
                    result = await task;
                }
                else
                {
                    // timeout/cancellation logic
                    task.ContinueWith(async action =>
                    {
                        await AfterRunFinished(run.Id, action, runFolder);
                    }, TaskContinuationOptions.None).SupressError();

                    return new RunResult()
                    {
                        RunId = run.Id,
                        Result = run.Result
                    };
                }

                var zipFile = await SaveSuccessRun(runFolder, run, result);

                await uow.CommitAsync();

                return new RunResult()
                {
                    FileName = zipFile.FileName,
                    StdOut = result.StdOut,
                    StdErr = result.StdError,
                    RunId = run.Id,
                    Duration = (result.FinishedAt - result.StartedAt).TotalSeconds
                };
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

                if (!_fileProvider.Exists(filePath))
                {
                    throw new NotFoundException("Given algorithm execution file doesn't exists");
                }

                var arguments = new List<string>() {"--help"};
                return await _processService.RunAsync(filePath, arguments, CancellationToken.None);
            }
        }

        private async Task AfterRunFinished(int runId, Task<ProcessResult> action, string runFolder)
        {
            using (var uow = UnitOfWorkProvider.Create(DbContextOptions.AlwaysCreateOwnContext))
            {
                var run = await _runRepository.GetById(runId);

                if (action.Status == TaskStatus.Canceled)
                {
                    run.Result = RunResultType.TimeOut;
                }
                else if (action.Status == TaskStatus.RanToCompletion)
                {
                    await SaveSuccessRun(runFolder, run, action.Result);
                }

                await uow.CommitAsync();
            }
        }

        private async Task<BasicFileInfo> SaveSuccessRun(string runFolder, Run run, ProcessResult result)
        {
            using (var uow = UnitOfWorkProvider.Create())
            {
                var zipFile = new BasicFileInfo();
                if (!_fileProvider.IsEmpty(runFolder))
                {
                    zipFile = _fileSystemProvider.GetTemporaryFile();
                    ZipFile.CreateFromDirectory(runFolder, zipFile.FullPath, CompressionLevel.Fastest, false);

                    run.File = new File()
                    {
                        Path = zipFile.FullPath,
                        Type = FileType.Result,
                        UserId = CurrentUserProvider.Id
                    };
                }

                run.FinishedAt = result.FinishedAt;
                run.StdOut = result.StdOut;
                run.StdErr = result.StdError;
                run.Result = result.ExitCode == 0 ? RunResultType.Success : RunResultType.Fail;

                //_runRepository.Insert(run);
                await uow.CommitAsync();
                return zipFile;
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