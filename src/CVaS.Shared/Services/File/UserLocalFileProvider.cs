using System.IO;
using CVaS.Shared.Options;
using Microsoft.Extensions.Options;

namespace CVaS.Shared.Services.File
{
    public class UserLocalFileProvider
    {
        private const string UserFilesDirectoryName = "UserFiles";
        private readonly string userFilesDirectory;

        public UserLocalFileProvider(IOptions<DirectoryPathOptions> directoryPathOptions)
        {
            var temporaryDirectoryPath = directoryPathOptions.Value.Temporary ?? Path.GetTempPath();
            this.userFilesDirectory = Path.Combine(temporaryDirectoryPath, UserFilesDirectoryName);
        }

        public string CreatePath(int userId, string fileHash, string fileExtension)
        {
            return Path.Combine(userFilesDirectory, userId.ToString(), fileHash + fileExtension);
        }
    }
}