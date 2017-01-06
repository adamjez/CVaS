using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CVaS.BL.Core.Provider;
using CVaS.BL.Providers;
using CVaS.BL.Repositories;
using CVaS.BL.Services.File;
using CVaS.DAL.Model;

namespace CVaS.BL.Facades
{
    public class FileFacade : AppFacadeBase
    {
        private readonly FileRepository _fileRepository;
        private readonly TemporaryFileProvider _temporaryFileProvider;
        public FileFacade(IUnitOfWorkProvider unitOfWorkProvider, ICurrentUserProvider currentUserProvider, 
            FileRepository fileRepository, TemporaryFileProvider temporaryFileProvider) 
            : base(unitOfWorkProvider, currentUserProvider)
        {
            _fileRepository = fileRepository;
            _temporaryFileProvider = temporaryFileProvider;
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

                _temporaryFileProvider.Delete(file.Path);
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
                });

                uow.Context.Files.AddRange(files);

                await uow.CommitAsync();

                return files;
            }
        }
    }
}