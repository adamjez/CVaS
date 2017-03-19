using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CVaS.DAL.Model;
using CVaS.Shared.Core;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Exceptions;
using CVaS.Shared.Helpers;
using CVaS.Shared.Models;
using CVaS.Shared.Options;
using CVaS.Shared.Repositories;
using CVaS.Shared.Services.File;
using CVaS.Shared.Services.File.Algorithm;
using CVaS.Shared.Services.File.Providers;
using CVaS.Shared.Services.File.Temporary;
using CVaS.Shared.Services.Process;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CVaS.Shared.Services.Launch
{
    public class LocalLaunchService : ILaunchService
    {
        private readonly IOptions<AlgorithmOptions> _options;

        private readonly IProcessService _processService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly RunRepository _runRepository;
        private readonly ITemporaryFileProvider _temporaryFileProvider;
        private readonly IUserFileProvider _userFileProvider;
        private readonly IAlgorithmFileProvider _algorithmFileProvider;
        private readonly ILogger<LocalLaunchService> _logger;
        private readonly FileSystemWrapper _fileSystemWrapper;

        public LocalLaunchService(IOptions<AlgorithmOptions> options, IProcessService processService, IUnitOfWorkProvider unitOfWorkProvider,
            RunRepository runRepository, ITemporaryFileProvider temporaryFileProvider, IUserFileProvider userFileProvider, IAlgorithmFileProvider algorithmFileProvider, 
            ILogger<LocalLaunchService> logger, 
            FileSystemWrapper fileSystemWrapper)
        {
            _options = options;
            _processService = processService;
            _unitOfWorkProvider = unitOfWorkProvider;
            _runRepository = runRepository;

            _temporaryFileProvider = temporaryFileProvider;
            _userFileProvider = userFileProvider;
            _algorithmFileProvider = algorithmFileProvider;
            _logger = logger;
            _fileSystemWrapper = fileSystemWrapper;
        }


        public async Task<RunResult> LaunchAsync(string codeName, string pathFile, List<Argument.Argument> args, Run run)
        {
            var filePath = _algorithmFileProvider.GetAlgorithmFilePath(codeName, pathFile);

            if (!_fileSystemWrapper.ExistsFile(filePath))
            {
                throw new NotFoundException("Given algorithm execution file doesn't exists");
            }

            using (var uow = _unitOfWorkProvider.Create())
            {
                // Download and Transform file arguments
                await _algorithmFileProvider.DownloadFiles(args, run.UserId);

                var stringArguments = args.Select(arg => arg.ToString()).ToList();

                var runFolder = _temporaryFileProvider.CreateTemporaryFolder();
                stringArguments.Insert(0, runFolder);

                var tokenSource = new CancellationTokenSource(_options.Value.HardTimeoutInSeconds * 1000);

                var task = _processService.RunAsync(filePath, stringArguments, tokenSource.Token);

                var result = await task.WithTimeout(TimeSpan.FromSeconds(_options.Value.LightTimeoutInSeconds));
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

                var savedRun = await SaveSuccessRun(runFolder, run, result.Value);

                await uow.CommitAsync();

                return new RunResult()
                {
                    FileId = savedRun.FileId,
                    StdOut = result.Value.StdOut,
                    StdErr = result.Value.StdError,
                    RunId = run.Id,
                    Result = savedRun.Result,
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

        private async Task<Run> SaveSuccessRun(string runFolder, Run run, ProcessResult result)
        {
            using (var uow = _unitOfWorkProvider.Create())
            {
                if (!_fileSystemWrapper.IsEmpty(runFolder))
                {
                    run.File = await CreateZipPackage(runFolder, run.UserId);
                }

                run.FinishedAt = result.FinishedAt;
                run.StdOut = result.StdOut;
                run.StdErr = result.StdError;
                run.Result = result.ExitCode == 0 ? RunResultType.Success : RunResultType.Fail;

                // Attach run, bcs we are not sure if it comes from same context
                _runRepository.Update(run);

                await uow.CommitAsync();
                return run;
            }
        }

        private async Task<DAL.Model.File> CreateZipPackage(string runFolder, int userId)
        {
            _logger.LogInformation("Creating zip file from result folder");

            var zipFile = _temporaryFileProvider.CreateTemporaryFilePath(ZipHelpers.Extension);
            ZipFile.CreateFromDirectory(runFolder, zipFile.FullPath, CompressionLevel.Fastest, false);

            return new DAL.Model.File()
            {
                Path = await _userFileProvider.SaveAsync(zipFile.FullPath, ZipHelpers.ContentType),
                Type = FileType.Result,
                ContentType = ZipHelpers.ContentType,
                Extension = ZipHelpers.Extension,
                UserId = userId
            };
        }
    }
}