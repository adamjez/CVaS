using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using CVaS.DAL.Model;
using CVaS.Shared.Core;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Helpers;
using CVaS.Shared.Models;
using CVaS.Shared.Options;
using CVaS.Shared.Repositories;
using CVaS.Shared.Services.File;
using CVaS.Shared.Services.Process;
using Microsoft.Extensions.Options;

namespace CVaS.Shared.Services.Launch
{
    public class LocalLaunchService : ILaunchService
    {
        private readonly IOptions<AlgorithmOptions> _options;

        private readonly IProcessService _processService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly RunRepository _runRepository;
        private readonly TemporaryFileProvider _fileSystemProvider;
        private readonly FileProvider _fileProvider;

        public LocalLaunchService(IOptions<AlgorithmOptions> options, IProcessService processService, IUnitOfWorkProvider unitOfWorkProvider,
            RunRepository runRepository, TemporaryFileProvider fileSystemProvider, FileProvider fileProvider)
        {
            _options = options;
            _processService = processService;
            _unitOfWorkProvider = unitOfWorkProvider;
            _runRepository = runRepository;
            _fileSystemProvider = fileSystemProvider;
            _fileProvider = fileProvider;
        }

        public async Task<RunResult> LaunchAsync(string filePath, List<string> args, Run run)
        {
            using (var uow = _unitOfWorkProvider.Create())
            {
                var runFolder = _fileSystemProvider.CreateTemporaryFolder();
                args.Insert(0, runFolder);

                var tokenSource = new CancellationTokenSource(_options.Value.HardTimeout * 1000);

                var task = _processService.RunAsync(filePath, args, tokenSource.Token);


                var result = await task.WithTimeout(TimeSpan.FromSeconds(_options.Value.LightTimeout));
                if (!result.Completed)
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

                var zipFile = await SaveSuccessRun(runFolder, run, result.Value);

                await uow.CommitAsync();

                return new RunResult()
                {
                    FileName = zipFile.FileName,
                    StdOut = result.Value.StdOut,
                    StdErr = result.Value.StdError,
                    RunId = run.Id,
                    Duration = (result.Value.FinishedAt - result.Value.StartedAt).TotalMilliseconds
                };
            }
        }

        private async Task AfterRunFinished(int runId, Task<ProcessResult> action, string runFolder)
        {
            using (var uow = _unitOfWorkProvider.Create(DbContextOptions.AlwaysCreateOwnContext))
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
            using (var uow = _unitOfWorkProvider.Create())
            {
                var zipFile = new BasicFileInfo();
                if (!_fileProvider.IsEmpty(runFolder))
                {
                    zipFile = _fileSystemProvider.GetTemporaryFile();
                    ZipFile.CreateFromDirectory(runFolder, zipFile.FullPath, CompressionLevel.Fastest, false);

                    run.File = new DAL.Model.File()
                    {
                        Path = zipFile.FullPath,
                        Type = FileType.Result,
                        UserId = run.UserId
                    };
                }

                run.FinishedAt = result.FinishedAt;
                run.StdOut = result.StdOut;
                run.StdErr = result.StdError;
                run.Result = result.ExitCode == 0 ? RunResultType.Success : RunResultType.Fail;

                // Attach run, bcs we are not sure if it comes from same context
                _runRepository.Update(run);

                await uow.CommitAsync();
                return zipFile;
            }
        }
    }
}