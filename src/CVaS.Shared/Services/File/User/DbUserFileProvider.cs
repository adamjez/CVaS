using System.IO;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace CVaS.Shared.Services.File.Providers
{
    public class DbUserFileProvider : UserFileProvider
    {
        private const string ContentType = "contentType";
        private readonly GridFSBucket _bucket;

        public DbUserFileProvider(IMongoDatabase database)
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

        public override async Task<FileResult> Get(string id)
        {
            ObjectId oid = new ObjectId(id);

            var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", oid);

            var result = (await _bucket.FindAsync(filter)).Single();

            var memStream = new MemoryStream();
            await _bucket.DownloadToStreamAsync(oid, memStream);
            memStream.Seek(0, SeekOrigin.Begin);

            return new FileResult(memStream, result.Filename, (string)result.Metadata[ContentType]);
        }

        public override async Task DeleteAsync(string id)
        {
            ObjectId oid = new ObjectId(id);

            await _bucket.DeleteAsync(oid);
        }
    }
}