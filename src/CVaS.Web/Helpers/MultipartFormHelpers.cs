using System;
using System.IO;
using System.Linq;

namespace CVaS.Web.Helpers
{
    /// <summary>
    /// Helps parse file inputs in multipart/formdata
    /// </summary>
    internal static class MultipartFormHelpers
    {
        public static bool IsMultipartContentType(string contentType)
        {
            return
                !string.IsNullOrEmpty(contentType) &&
                contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string GetExtension(string contentDisposation)
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

                return Path.GetExtension(fileName);
            }

            return string.Empty;
        }

        public static string GetBoundary(string contentType)
        {
            var elements = contentType.Split(' ');
            var element = elements.First(entry => entry.StartsWith("boundary="));
            var boundary = element.Substring("boundary=".Length);
            // Remove quotes
            if (boundary.Length >= 2 && boundary[0] == '"' &&
                boundary[boundary.Length - 1] == '"')
            {
                boundary = boundary.Substring(1, boundary.Length - 2);
            }
            return boundary;
        }
    }
}