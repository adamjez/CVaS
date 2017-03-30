using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace CVaS.Shared.Services.File.User
{
    public class AzureBlobStorage : FileStorage
    {
        private readonly CloudBlobClient _blobClient;

        public AzureBlobStorage(CloudStorageAccount storageAccount)
        {
            _blobClient = storageAccount.CreateCloudBlobClient();
        }

        public override async Task<string> SaveAsync(Stream stream, string fileName, string contentType)
        {
            var container = await InitializeContainer();

            var guid = Guid.NewGuid();
            CloudBlockBlob blob = container.GetBlockBlobReference(guid.ToString());

            blob.Properties.ContentType = contentType;

            await blob.UploadFromStreamAsync(stream);

            return guid.ToString();
        }

        public override async Task<FileResult> GetAsync(string id)
        {
            var container = await InitializeContainer();

            var blob = container.GetBlockBlobReference(id);
        
            var memStream = new MemoryStream();
            await blob.DownloadToStreamAsync(memStream);
            memStream.Seek(0, SeekOrigin.Begin);

            return new FileResult(memStream, blob.Properties.ContentType);
        }

        public override async Task DeleteAsync(string id)
        {
            var container = await InitializeContainer();

            var blob = container.GetBlockBlobReference(id);

            await blob.DeleteIfExistsAsync();
        }

        private async Task<CloudBlobContainer> InitializeContainer()
        {
            // Retrieve a reference to a container.
            CloudBlobContainer container = _blobClient.GetContainerReference("userfiles");

            // Create the container if it doesn't already exist.
            await container.CreateIfNotExistsAsync();

            return container;
        }
    }
}