﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CVaS.BL.Exceptions;
using CVaS.BL.Providers;
using CVaS.BL.Repositories;
using CVaS.DAL;
using CVaS.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Logging;
using FileResult = CVaS.Web.Models.FileResult;

namespace CVaS.Web.Controllers
{
    [Route("[controller]")]
    public class FileController : ApiController
    {
        private readonly ILogger<FileController> _logger;
        private readonly TempFileProvider _fileProvider;
        private readonly FileRepository _fileRepository;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly AppDbContext _context;

        public FileController(ILogger<FileController> logger, TempFileProvider fileProvider, FileRepository fileRepository, 
            ICurrentUserProvider currentUserProvider, AppDbContext context)
        {
            _logger = logger;
            this._fileProvider = fileProvider;
            this._fileRepository = fileRepository;
            _currentUserProvider = currentUserProvider;
            _context = context;
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
                await _fileProvider.CreateTempFile(image);
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

            var files = new List<DAL.Model.File>();
            while (section != null)
            {
                var filename = _fileProvider.CreateFileName();
                using (var stream = _fileProvider.CreateTempFile1(filename))
                {
                    await section.Body.CopyToAsync(stream);
                }

                files.Add(new DAL.Model.File()
                {
                    Path = filename,
                    UserId = _currentUserProvider.Id
                });

                section = await reader.ReadNextSectionAsync();
            }

            _context.Files.AddRange(files);
            await _context.SaveChangesAsync();

            return Ok(files);
        }


        [HttpGet("1/{zipName}")]
        public async Task GetResultZip(string zipName)
        {
            const string zipMime = "application/zip";
            HttpContext.Response.ContentType = zipMime;
            var path = Path.Combine(@"D:\Downloads", zipName);

            var service = HttpContext.Features.Get<IHttpSendFileFeature>();
            IFileInfo f = new PhysicalFileInfo(new FileInfo(path));

            if (service != null)
            {
                //await service.SendFileAsync(path, 0, f.Length, CancellationToken.None);
                //return StatusCode(200);
            }
            await HttpContext.Response.SendFileAsync(f);

            //return PhysicalFile(path, zipMime);
        }

        [HttpGet("2/{zipName}")]
        public IActionResult GetResultZip1(string zipName)
        {
            const string zipMime = "application/zip";
            HttpContext.Response.ContentType = zipMime;
            var path = Path.Combine(@"D:\Downloads", zipName);     

            return PhysicalFile(path, zipMime);
        }

        // Nejlepsi verze !!!!!!!!!
        [HttpGet("3/{zipName}")]
        public async Task GetResultZip2(string zipName)
        {
            const string zipMime = "application/zip";
            HttpContext.Response.ContentType = zipMime;
            var path = Path.Combine(@"D:\Downloads", zipName);

            var service = HttpContext.Features.Get<IHttpSendFileFeature>();
            IFileInfo f = new PhysicalFileInfo(new FileInfo(path));

            if (service != null)
            {
                await service.SendFileAsync(path, 0, f.Length, CancellationToken.None);
            }
            else
            {
                await HttpContext.Response.SendFileAsync(f);

            }
        }


        [HttpGet("4/{zipName}")]
        public Microsoft.AspNetCore.Mvc.FileResult GetResultZip3(string zipName)
        {
            const string zipMime = "application/zip";
            HttpContext.Response.ContentType = zipMime;
            var path = Path.Combine(@"D:\Downloads", zipName);

            HttpContext.Response.ContentType = zipMime;
            FileContentResult result = new FileContentResult(System.IO.File.ReadAllBytes(path), zipMime)
            {
                FileDownloadName = zipName
            };

            return result;
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
    }
}