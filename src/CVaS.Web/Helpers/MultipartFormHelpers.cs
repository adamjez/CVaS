using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace CVaS.Web.Helpers
{
    /// <summary>
    /// Helps parse file inputs in multipart/formdata
    /// </summary>
    internal static class MultipartFormHelpers
    {
        public static bool IsMultipartContentType(this HttpRequest request)
        {
            var contentType = request.ContentType;

            return
                !string.IsNullOrEmpty(contentType) &&
                contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string GetFileName(string contentDisposation)
        {
            // ContentDisposation= form-data; name="file"; filename="us-1.jpg"

            const string filename = "filename=";
            int index = contentDisposation.LastIndexOf(filename, StringComparison.OrdinalIgnoreCase);
            if (index > -1)
            {
                var fileName = contentDisposation
                    .Substring(index + filename.Length)
                    .Split(';')
                    .First()
                    .Replace("\"", "");

                return fileName;
            }

            return string.Empty;
        }
    }
}