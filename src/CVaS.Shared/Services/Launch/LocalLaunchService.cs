using System;
using System.IO;
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
using CVaS.Shared.Services.File.Temporary;
using CVaS.Shared.Services.File.User;
using CVaS.Shared.Services.Process;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeTypes;

namespace CVaS.Shared.Services.Launch
{
    public class LocalLaunchService : ILaunchService
    {
        private readonly IOptions<AlgorithmOptions> _options;

        private readonly IProcessService _processService;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly RunRepository _runRepository;
        private readonly ITemporaryFileProvider _temporaryFileProvider;
        private readonly IFileStorage _fileStorage;
        private readonly IAlgorithmFileProvider _algorithmFileProvider;
        private readonly ILogger<LocalLaunchService> _logger;
        private readonly FileSystemWrapper _fileSystemWrapper;

        public LocalLaunchService(IOptions<AlgorithmOptions> options, IProcessService processService, IUnitOfWorkProvider unitOfWorkProvider,
            RunRepository runRepository, ITemporaryFileProvider temporaryFileProvider, IFileStorage fileStorage, IAlgorithmFileProvider algorithmFileProvider,
            ILogger<LocalLaunchService> logger, FileSystemWrapper fileSystemWrapper)
        {
            _options = options;
            _processService = processService;
            _unitOfWorkProvider = unitOfWorkProvider;
            _runRepository = runRepository;

            _temporaryFileProvider = temporaryFileProvider;
            _fileStorage = fileStorage;
            _algorithmFileProvider = algorithmFileProvider;
            _logger = logger;
            _fileSystemWrapper = fileSystemWrapper;
        }


        public async Task<RunResult> LaunchAsync(Algorithm algorithm, Run run, RunSettings settings)
        {
            var filePath = _algorithmFileProvider.GetAlgorithmFilePath(algorithm.CodeName, algorithm.FilePath);

            if (!_fileSystemWrapper.ExistsFile(filePath))
            {
                throw new NotFoundException("Given algorithm execution file doesn't exists");
            }

            using (var uow = _unitOfWorkProvider.Create())
            {
                // Download and Transform file arguments
                await _algorithmFileProvider.DownloadFiles(settings.Arguments, run.UserId);

                var stringArguments = settings.Arguments.Select(arg => arg.ToString()).ToList();

                var runFolder = _temporaryFileProvider.CreateTemporaryFolder();

                var tokenSource = new CancellationTokenSource(_options.Value.HardTimeoutInSeconds * 1000);

                var task = _processService.RunAsync(new ProcessOptions(filePath, stringArguments, runFolder), tokenSource.Token);

                var lightTimeout = !settings.Timeout.HasValue || settings.Timeout < 0
                    ? _options.Value.LightTimeoutInSeconds
                    : settings.Timeout.Value;

                var result = await task.WithTimeout(TimeSpan.FromSeconds(lightTimeout));
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
                        Result = run.Result,
                        CreatedAt = run.CreatedAt
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
                    CreatedAt = result.Value.StartedAt,
                    FinishedAt = result.Value.FinishedAt
                };
            }
        }

        private async Task AfterRunFinished(Guid runId, Task<ProcessResult> action, string runFolder)
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
                var filesCount = _fileSystemWrapper.FilesCountInFolder(runFolder);
                if (filesCount == 1)
                {
                    run.File = await CreateSingleFile(runFolder, run.UserId);
                }
                else if (filesCount > 1)
                {
                    run.File = await CreateZipPackage(runFolder, run.UserId);
                }

                run.FinishedAt = result.FinishedAt;
                run.StartedAt = result.StartedAt;
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
                LocationId = await _fileStorage.SaveAsync(zipFile.FullPath, ZipHelpers.ContentType),
                FileSize = _fileSystemWrapper.FileSize(zipFile.FullPath),
                ContentType = ZipHelpers.ContentType,
                Extension = ZipHelpers.Extension,
                Type = FileType.Result,
                UserId = userId,
            };
        }

        private async Task<DAL.Model.File> CreateSingleFile(string runFolder, int userId)
        {
            _logger.LogInformation("Uploading single file from result folder");

            var filePath = Directory.EnumerateFiles(runFolder).First();
            var fileInfo = new System.IO.FileInfo(filePath);
            var contetType = MimeTypeMap.GetMimeType(fileInfo.Extension);

            return new DAL.Model.File()
            {
                LocationId = await _fileStorage.SaveAsync(filePath, contetType),
                FileSize = _fileSystemWrapper.FileSize(filePath),
                ContentType = contetType,
                Extension = fileInfo.Extension,
                Type = FileType.Result,
                UserId = userId,
            };
        }
    }
}