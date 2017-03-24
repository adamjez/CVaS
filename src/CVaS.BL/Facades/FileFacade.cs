using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Providers;
using CVaS.Shared.Repositories;
using CVaS.Shared.Services.File.Providers;
using File = CVaS.DAL.Model.File;

namespace CVaS.BL.Facades
{
    public class FileFacade : AppFacadeBase
    {
        private readonly FileRepository _fileRepository;
        private readonly IUserFileProvider _userFileProvider;
        public FileFacade(IUnitOfWorkProvider unitOfWorkProvider, ICurrentUserProvider currentUserProvider,
            FileRepository fileRepository, IUserFileProvider userFileProvider)
            : base(unitOfWorkProvider, currentUserProvider)
        {
            _fileRepository = fileRepository;
            _userFileProvider = userFileProvider;
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

                await _userFileProvider.DeleteAsync(file.LocationId);

                _fileRepository.Delete(file);
                await uow.CommitAsync();
            }
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

                        var path = await _userFileProvider.SaveAsync(memStream, fileName, contentType);

                        var fileEntity = new File()
                        {
                            LocationId = path,
                            UserId = CurrentUserProvider.Id,
                            Type = FileType.Upload,
                            Hash = hash,
                            Extension = Path.GetExtension(fileName),
                            ContentType = contentType,
                            FileSize = memStream.Length
                        };

                        uow.Context.Files.Add(fileEntity);

                        await uow.CommitAsync();

                        return fileEntity.Id;
                    }
                }
            }
        }

        public async Task<IEnumerable<File>> AddUploadedAsync(IEnumerable<string> pathFiles, int userId)
        {
            using (var uow = UnitOfWorkProvider.Create())
            {
                var files = pathFiles.Select(path => new File()
                {
                    LocationId = path,
                    UserId = userId,
                    Type = FileType.Upload
                }).ToList();

                uow.Context.Files.AddRange(files);

                await uow.CommitAsync();

                return files;
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
    }
}