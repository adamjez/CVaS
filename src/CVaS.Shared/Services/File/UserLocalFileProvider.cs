using System.IO;
using CVaS.Shared.Options;
using Microsoft.Extensions.Options;
using System;

namespace CVaS.Shared.Services.File
{
    public class UserLocalFileProvider
    {
        private const string UserFilesDirectoryName = "UserFiles";
        private readonly string userFilesDirectory;
        private readonly FileSystemWrapper _fileSystemWrapper;

        public UserLocalFileProvider(IOptions<DirectoryPathOptions> directoryPathOptions, FileSystemWrapper fileSystemWrapper)
        {
            _fileSystemWrapper = fileSystemWrapper;
            var temporaryDirectoryPath = directoryPathOptions.Value.Temporary ?? Path.GetTempPath();
            this.userFilesDirectory = Path.Combine(temporaryDirectoryPath, UserFilesDirectoryName);
        }

        public string CreatePath(int userId, byte[] fileHash, string fileExtension)
        {
            var pathToUserDirectory = Path.Combine(userFilesDirectory, userId.ToString());

            if (!_fileSystemWrapper.ExistsDirectory(pathToUserDirectory))
            {
                _fileSystemWrapper.CreateDirectory(pathToUserDirectory);
            }

            var fileHashPathFriendly = BitConverter.ToString(fileHash).Replace("-", "").ToLower();

            return Path.Combine(pathToUserDirectory, fileHashPathFriendly + fileExtension);
        }
    }
}