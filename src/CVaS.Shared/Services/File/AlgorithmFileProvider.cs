using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Exceptions;
using CVaS.Shared.Repositories;
using CVaS.Shared.Services.Argument;
using CVaS.Shared.Services.File.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CVaS.Shared.Services.File
{
    public class AlgorithmFileProvider : IAlgorithmFileProvider
    {
        private readonly IConfigurationRoot configuration;
        private readonly ILogger<AlgorithmFileProvider> _logger;
        private readonly FileRepository _fileRepository;
        private readonly IFileProvider _fileProvider;
        private readonly TemporaryFileProvider _fileSystemProvider;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;

        public AlgorithmFileProvider(IConfigurationRoot configuration, ILogger<AlgorithmFileProvider> logger, FileRepository fileRepository,
            IFileProvider fileProvider, TemporaryFileProvider fileSystemProvider, IUnitOfWorkProvider unitOfWorkProvider)
        {
            this.configuration = configuration;
            _logger = logger;
            _fileRepository = fileRepository;
            _fileProvider = fileProvider;
            _fileSystemProvider = fileSystemProvider;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        public string GetAlgorithmFilePath(string codeName, string algFile)
        {
            var pathToFolder = Path.Combine(configuration["DirectoryPaths:Algorithm"], codeName);
            var pathToFile = Path.Combine(pathToFolder, algFile);

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

                var tasks = new List<Task<FileArgumentDownloadResult>>();
                foreach (var dbFile in filesEntities)
                {
                    if (dbFile.UserId != userId)
                    {
                        throw new UnauthorizedAccessException();
                    }
                    var localId = dbFile.Id;
                    var resultTask = _fileProvider.Get(dbFile.Path)
                        .ContinueWith(async t => await SaveFileLocally(await t, localId))
                        .Unwrap();

                    tasks.Add(resultTask);
                }

                var filesSaved = await Task.WhenAll(tasks);

                foreach (var file in files)
                {
                    file.LocalPath = filesSaved.Single(f => f.Id == file.FileId).LocalPath;
                }
            }
        }

        private async Task<FileArgumentDownloadResult> SaveFileLocally(FileResult fileResult, int localId)
        {
            string path;

            using (fileResult.Content)
            using (var file = _fileSystemProvider.CreateTemporaryFile(Path.GetExtension(fileResult.FileName), out path))
                await fileResult.Content.CopyToAsync(file);

            return new FileArgumentDownloadResult(path, localId);
        }

        private struct FileArgumentDownloadResult
        {
            public FileArgumentDownloadResult(string localPath, int id)
            {
                LocalPath = localPath;
                Id = id;
            }

            public string LocalPath { get; }
            public int Id { get; }
        }
    }
}

