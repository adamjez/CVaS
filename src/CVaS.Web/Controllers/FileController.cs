using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CVaS.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace CVaS.Web.Controllers
{
    [Route("[controller]")]
    public class FileController : ApiController
    {
        private readonly ILogger<FileController> _logger;
        private readonly TempFileProvider fileProvider;

        public FileController(ILogger<FileController> logger, TempFileProvider fileProvider)
        {
            _logger = logger;
            this.fileProvider = fileProvider;
        }

        [HttpPost("")]
        public async Task<IActionResult> UploadSingleFile(IFormFile image)
        {
            //if (!files.Any())
            //{
            //    return BadRequest("You have to upload atleast 1 file");
            //}

            //foreach (var file in files)
            {
                await fileProvider.CreateTempFile(image);
            }

            return Ok(new FileResult
            {
                //FileNames = files.Select(x => x.FileName),
                //ContentTypes = files.Select(x => x.ContentType),
                CreatedTimestamp = DateTime.UtcNow,
                UpdatedTimestamp = DateTime.UtcNow,
                DownloadLink = image.FileName
            });
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadMultipleFiles()
        {
            if (!IsMultipartContentType(HttpContext.Request.ContentType))
            {
                return BadRequest();
            }

            var boundary = GetBoundary(HttpContext.Request.ContentType);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();

            var fileNames = new List<string>();
            while (section != null)
            {
                // process each image
                const int chunkSize = 1024;
                var buffer = new byte[chunkSize];

                var filename = fileProvider.CreateFileName();
                using (var stream = fileProvider.CreateTempFile1(filename))
                {
                    var bytesRead = 0;
                    do
                    {
                        bytesRead = await section.Body.ReadAsync(buffer, 0, buffer.Length);
                        stream.Write(buffer, 0, bytesRead);

                    } while (bytesRead > 0);
                }

                fileNames.Add(filename);
                section = await reader.ReadNextSectionAsync();
            }

            return Ok(fileNames);
        }

        private static bool IsMultipartContentType(string contentType)
        {
            return
                !string.IsNullOrEmpty(contentType) &&
                contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string GetBoundary(string contentType)
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

        public class FileResult
        {
            public IEnumerable<string> FileNames { get; set; }
            public string Description { get; set; }
            public DateTime CreatedTimestamp { get; set; }
            public DateTime UpdatedTimestamp { get; set; }
            public string DownloadLink { get; set; }
            public IEnumerable<string> ContentTypes { get; set; }
        }

    }
}