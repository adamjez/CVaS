using System.IO;

namespace CVaS.Shared.Services.File.Providers
{
    public struct FileResult
    {
        public FileResult(Stream content, string fileName, string contentType = null)
        {
            Content = content;
            FileName = fileName;
            ContentType = contentType;
        }

        public Stream Content { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}