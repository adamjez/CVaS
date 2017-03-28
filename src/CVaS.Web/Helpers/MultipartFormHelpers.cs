using System;
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

        public static string GetBoundary(string contentType)
        {
            var elements = contentType.Split(' ');
            var element = elements.FirstOrDefault(entry => entry.StartsWith("boundary="));
            if (element == null)
                return null;
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