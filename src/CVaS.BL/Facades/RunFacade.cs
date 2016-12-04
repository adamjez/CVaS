using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading.Tasks;
using CVaS.BL.Core.Provider;
using CVaS.BL.Exceptions;
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
            FileProvider fileProvider, AlgorithmFileProvider algFileProvider, TemporaryFileProvider fileSystemProvider, RunRepository runRepository)
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

                args.Insert(0, runFolder);
                var result = _processService.Run(filePath, _fileProvider.GetDirectoryFromFile(filePath), args);

                var zipFile = new BasicFileInfo();
                if (!_fileProvider.IsEmpty(runFolder))
                {
                    zipFile = _fileSystemProvider.GetTemporaryFile();
                    ZipFile.CreateFromDirectory(runFolder, zipFile.FullPath, CompressionLevel.Fastest, false);
                }
                var run = new Run()
                {
                    Path = zipFile.FullPath,
                    AlgorithmId = algorithm.Id,
                    UserId = CurrentUserProvider.Id,
                    StdOut = result.StdOut,
                    StdErr = result.StdError
                };

                _runRepository.Insert(run);
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

        public async Task<Run> GetSafely(int runId)
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