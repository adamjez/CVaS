using System.IO;

namespace CVaS.Shared.Services.File.Providers
{
    public struct FileResult
    {
        public FileResult(Stream content, string contentType = null)
        {
            Content = content;
            ContentType = contentType;
        }

        public Stream Content { get; set; }
        public string ContentType { get; set; }
    }
}