using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Providers;
using CVaS.Shared.Repositories;
using CVaS.Shared.Services.File;
using CVaS.Shared.Services.File.Providers;

namespace CVaS.BL.Facades
{
    public class FileFacade : AppFacadeBase
    {
        private readonly FileRepository _fileRepository;
        private readonly IFileProvider _fileProvider;
        public FileFacade(IUnitOfWorkProvider unitOfWorkProvider, ICurrentUserProvider currentUserProvider, 
            FileRepository fileRepository, IFileProvider fileProvider) 
            : base(unitOfWorkProvider, currentUserProvider)
        {
            _fileRepository = fileRepository;
            _fileProvider = fileProvider;
        }

        public async Task DeleteAsync(int fileId, int userId)
        {
            using (UnitOfWorkProvider.Create())
            {
                var file = await _fileRepository.GetByIdSafely(fileId);

                if (file.UserId != userId)
                {
                    throw new UnauthorizedAccessException();
                }

                await _fileProvider.DeleteAsync(file.Path);
            }
        }

        public async Task<IEnumerable<File>> AddUploadedAsync(IEnumerable<string> pathFiles, int userId)
        {
            using (var uow = UnitOfWorkProvider.Create())
            {
                var files = pathFiles.Select(path => new File()
                {
                    Path = path,
                    UserId = userId,
                    Type = FileType.Upload
                }).ToList();

                uow.Context.Files.AddRange(files);

                await uow.CommitAsync();

                return files;
            }
        }

        public async Task<string> GetSafelyAsync(int fileId)
        {
            using (UnitOfWorkProvider.Create())
            {
                var file = await _fileRepository.GetByIdSafely(fileId);

                if (file.UserId != CurrentUserProvider.Id)
                {
                    throw new UnauthorizedAccessException();
                }

                return file.Path;
            }
        }
    }
}