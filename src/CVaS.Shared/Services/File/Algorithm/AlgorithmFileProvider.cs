using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Exceptions;
using CVaS.Shared.Options;
using CVaS.Shared.Repositories;
using CVaS.Shared.Services.Argument;
using CVaS.Shared.Services.File.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CVaS.Shared.Services.File.Algorithm
{
    public class AlgorithmFileProvider : IAlgorithmFileProvider
    {
        private readonly IOptions<DirectoryPathOptions> _directoryPathOptions;
        private readonly ILogger<AlgorithmFileProvider> _logger;
        private readonly FileRepository _fileRepository;
        private readonly IUserFileProvider _userFileProvider;
        private readonly FileSystemWrapper _fileSystem;
        private readonly UserLocalFileProvider _userLocalFileProvider;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public AlgorithmFileProvider(IOptions<DirectoryPathOptions> directoryPathOptions, ILogger<AlgorithmFileProvider> logger, FileRepository fileRepository,
            IUserFileProvider userFileProvider, FileSystemWrapper fileSystem, IUnitOfWorkProvider unitOfWorkProvider, UserLocalFileProvider userLocalFileProvider)
        {
            _directoryPathOptions = directoryPathOptions;
            _logger = logger;
            _fileRepository = fileRepository;
            _userFileProvider = userFileProvider;
            _fileSystem = fileSystem;
            _unitOfWorkProvider = unitOfWorkProvider;
            _userLocalFileProvider = userLocalFileProvider;
        }

        public string GetAlgorithmFilePath(string codeName, string algFile)
        {
            var pathToFile = Path.Combine(_directoryPathOptions.Value.Algorithm, codeName, algFile);

            _logger.LogInformation("Creating file to path: " + pathToFile);

            return pathToFile;
        }

        public async Task DownloadFiles(List<Argument.Argument> arguments, int userId)
        {
            using (_unitOfWorkProvider.Create())
            {
                var files = arguments.OfType<FileArgument>().ToList();
                if (!files.Any())
                    return;

                var filesEntities = await _fileRepository.GetByIds(files.Select(f => f.FileId));

                // We didn't get all files => some files are missing
                if (files.Count != filesEntities.Count)
                {
                    throw new NotFoundException();
                }

                var tasks = new List<Task<LocalFileResult>>();
                foreach (var dbFile in filesEntities)
                {
                    tasks.Add(DownloadFile(dbFile, userId));
                }

                var filesSaved = await Task.WhenAll(tasks);

                foreach (var file in files)
                {
                    file.LocalPath = filesSaved.Single(f => f.Id == file.FileId).LocalPath;
                }
            }
        }

        private Task<LocalFileResult> DownloadFile(DAL.Model.File dbFile, int userId)
        {
            if (dbFile.UserId != userId)
            {
                throw new UnauthorizedAccessException();
            }

            // future Local File Path
            var localPath = _userLocalFileProvider.CreatePath(userId, dbFile.Hash.ToString(), dbFile.Extension);

            // Check if file exists locally
            if (System.IO.File.Exists(localPath))
            {
                return Task.FromResult(new LocalFileResult(localPath, dbFile.Id));
            }

            var localFile = dbFile;
            var resultTask = _userFileProvider.Get(dbFile.Path)
                .ContinueWith(async t =>
                {
                    await _fileSystem.SaveAsync((await t).Content, localPath);
                    return new LocalFileResult(localPath, localFile.Id);
                })
                .Unwrap();

            return resultTask;
        }

        private struct LocalFileResult
        {
            public LocalFileResult(string localPath, int id)
            {
                LocalPath = localPath;
                Id = id;
            }

            public string LocalPath { get; }
            public int Id { get; }
        }
    }
}

