using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CVaS.BL.DTO;
using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Repositories;
using File = CVaS.DAL.Model.File;
using CVaS.BL.Providers;
using CVaS.Shared.Services.File.User;
using CVaS.Shared.Core;

namespace CVaS.BL.Facades
{
    public class FileFacade : AppFacadeBase
    {
        private readonly FileRepository _fileRepository;
        private readonly IFileStorage _fileStorage;
        public FileFacade(IUnitOfWorkProvider unitOfWorkProvider, ICurrentUserProvider currentUserProvider,
            FileRepository fileRepository, IFileStorage fileStorage)
            : base(unitOfWorkProvider, currentUserProvider)
        {
            _fileRepository = fileRepository;
            _fileStorage = fileStorage;
        }

        public async Task<Guid> AddFileAsync(Stream fileStream, string fileName, string contentType)
        {
            using (var uow = UnitOfWorkProvider.Create())
            {
                // ToDo: find better solution, we need to intruduce new stream, bcs old stream is not seekable
                // and we need to read it once more
                using (var memStream = new MemoryStream())
                {
                    await fileStream.CopyToAsync(memStream);

                    memStream.Seek(0, SeekOrigin.Begin);

                    using (var md5Hash = System.Security.Cryptography.MD5.Create())
                    {
                        var hash = md5Hash.ComputeHash(memStream);
                        memStream.Seek(0, SeekOrigin.Begin);

                        var fileWithSameHash = await _fileRepository.GetByHashAsync(hash, CurrentUserProvider.Id);

                        if (fileWithSameHash != null)
                            return fileWithSameHash.Id;

                        var path = await _fileStorage.SaveAsync(memStream, fileName, contentType);

                        var fileEntity = new File()
                        {
                            LocationId = path,
                            UserId = CurrentUserProvider.Id,
                            Type = FileType.Upload,
                            Hash = hash,
                            ContentType = contentType,
                            FileSize = memStream.Length,
                            Extension = Path.GetExtension(fileName)
                        };

                        uow.Context.Files.Add(fileEntity);

                        await uow.CommitAsync();

                        return fileEntity.Id;
                    }
                }
            }
        }

        public async Task<IEnumerable<FileDTO>> GetAllBySignedUserAsync()
        {
            using (UnitOfWorkProvider.Create())
            {
                var files = await _fileRepository.GetByUser(CurrentUserProvider.Id);

                return files.Select(FileDTO.FromEntity);
            }
        }

        public async Task<File> GetSafelyAsync(Guid fileId)
        {
            using (UnitOfWorkProvider.Create())
            {
                var file = await _fileRepository.GetByIdSafely(fileId);

                if (file.UserId != CurrentUserProvider.Id)
                {
                    throw new UnauthorizedAccessException();
                }

                return file;
            }
        }

        public async Task DeleteAsync(Guid fileId)
        {
            using (var uow = UnitOfWorkProvider.Create())
            {
                var file = await _fileRepository.GetByIdSafely(fileId);

                if (file.UserId != CurrentUserProvider.Id)
                {
                    throw new UnauthorizedAccessException();
                }

                await _fileStorage.DeleteAsync(file.LocationId);

                _fileRepository.Delete(file);
                await uow.CommitAsync();
            }
        }

        public async Task DeleteAllAsync()
        {
            using (var uow = UnitOfWorkProvider.Create(DbContextOptions.ReuseParentContext))
            {
                var files = await _fileRepository.GetByUser(CurrentUserProvider.Id);

                foreach (var file in files)
                {
                    await DeleteAsync(file.Id);
                }

                await uow.CommitAsync();
            }
        }
    }
}