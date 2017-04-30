using System.IO;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace CVaS.Shared.Services.File.User
{
    public class MongoDbFileStorage : FileStorageBase
    {
        private const string ContentType = "contentType";
        private readonly GridFSBucket _bucket;

        public MongoDbFileStorage(IMongoDatabase database)
        {
            _bucket = new GridFSBucket(database);
        }

        public override async Task<string> SaveAsync(Stream stream, string fileName, string contentType)
        {
            var uploadOptions = new GridFSUploadOptions()
            {
                Metadata = new BsonDocument
                {
                    {ContentType, contentType},
                }
            };

            var newId = ObjectId.GenerateNewId();
            await _bucket.UploadFromStreamAsync(newId, fileName, stream, uploadOptions);

            return newId.ToString();
        }

        public override async Task<FileResult> GetAsync(string id)
        {
            ObjectId oid = new ObjectId(id);

            var stream = await _bucket.OpenDownloadStreamAsync(oid);

            return new FileResult(stream, (string)stream.FileInfo.Metadata[ContentType]);
        }

        public override async Task DeleteAsync(string id)
        {
            ObjectId oid = new ObjectId(id);

            await _bucket.DeleteAsync(oid);
        }
    }
}