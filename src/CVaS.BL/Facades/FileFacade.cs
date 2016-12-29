using System;
using System.Threading.Tasks;
using CVaS.BL.Core.Provider;
using CVaS.BL.Providers;
using CVaS.BL.Repositories;
using CVaS.BL.Services.File;

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
    }
}