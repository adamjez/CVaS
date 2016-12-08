using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using CVaS.BL.Core;
using CVaS.BL.Core.Provider;
using CVaS.BL.Exceptions;
using CVaS.BL.Helpers;
using CVaS.BL.Models;
using CVaS.BL.Providers;
using CVaS.BL.Repositories;
using CVaS.BL.Services.File;
using CVaS.BL.Services.Process;
using CVaS.DAL.Model;

namespace CVaS.BL.Facades
{
    public class RunFacade : AppFacadeBase
    {
        private readonly AlgorithmRepository _algorithmRepository;
        private readonly FileProvider _fileProvider;
        private readonly AlgorithmFileProvider _algFileProvider;
        private readonly IProcessService _processService;
        private readonly FileRepository _fileRepository;
        private readonly TemporaryFileProvider _fileSystemProvider;
        private readonly RunRepository _runRepository;

        public RunFacade(IUnitOfWorkProvider unitOfWorkProvider, ICurrentUserProvider currentUserProvider,
            AlgorithmRepository algorithmRepository, IProcessService processService, FileRepository fileRepository,
            FileProvider fileProvider, AlgorithmFileProvider algFileProvider, TemporaryFileProvider fileSystemProvider,
            RunRepository runRepository)
            : base(unitOfWorkProvider, currentUserProvider)
        {
            _algorithmRepository = algorithmRepository;
            this._processService = processService;
            _fileRepository = fileRepository;
            _fileProvider = fileProvider;
            this._algFileProvider = algFileProvider;
            _fileSystemProvider = fileSystemProvider;
            _runRepository = runRepository;
        }

        public async Task<RunResult> RunProcessAsync(string codeName, IEnumerable<string> arguments)
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

                List<string> args = new List<string>();
                foreach (var arg in arguments)
                {
                    args.Add(await ProcessArgument(arg));
                }

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

                int timeout = 10000;
                var tokenSource = new CancellationTokenSource(timeout);

                var task = _processService.RunAsync(filePath, _fileProvider.GetDirectoryFromFile(filePath), args,
                    tokenSource.Token);

                var lightTimeOut = 1000;
                var lightTokenSource = new CancellationTokenSource();
                var firstTimeOutedTask = await Task.WhenAny(task, Task.Delay(lightTimeOut, lightTokenSource.Token));

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
                    RunId = run.Id
                };
            }
        }

        private async Task AfterRunFinished(int runId, Task<ProcessResult> action, string runFolder)
        {
            using (var uow = UnitOfWorkProvider.Create())
            {
                var run = await _runRepository.GetById(runId);

                if (action.Status == TaskStatus.Canceled)
                {
                    // TimeOut
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
                }

                run.Path = zipFile.FullPath;
                run.StdOut = result.StdOut;
                run.StdErr = result.StdError;
                run.Result = result.ExitCode == 0 ? RunResultType.Success : RunResultType.Fail;

                //_runRepository.Insert(run);
                await uow.CommitAsync();
                return zipFile;
            }
        }

        private async Task<string> ProcessArgument(string arg)
        {
            //TODO localFile to scheme configuration
            if (arg.StartsWith("localFile://"))
            {
                //TODO Check for user
                var argEntity = await _fileRepository.GetById(int.Parse(arg.Substring("localFile://".Length)));

                if (argEntity.UserId != CurrentUserProvider.Id)
                {
                    throw new UnauthorizedAccessException();
                }

                return argEntity.Path;
            }
            else
            {
                //TODO Validate?
                return arg;
            }
        }

        public async Task<Run> GetSafelyAsync(int runId)
        {
            using (UnitOfWorkProvider.Create())
            {
                var run = await _runRepository.GetByIdSafely(runId);

                if (run.UserId != CurrentUserProvider.Id)
                {
                    throw new UnauthorizedAccessException();
                }

                return run;
            }
        }
    }
}