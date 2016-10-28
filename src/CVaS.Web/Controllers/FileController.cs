﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CVaS.BL.Exceptions;
using CVaS.BL.Providers;
using CVaS.BL.Repositories;
using CVaS.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
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

        public FileController(ILogger<FileController> logger, TempFileProvider fileProvider, FileRepository fileRepository, 
            ICurrentUserProvider currentUserProvider)
        {
            _logger = logger;
            this._fileProvider = fileProvider;
            this._fileRepository = fileRepository;
            _currentUserProvider = currentUserProvider;
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

            var fileNames = new List<string>();
            while (section != null)
            {
                var filename = _fileProvider.CreateFileName();
                using (var stream = _fileProvider.CreateTempFile1(filename))
                {
                    await section.Body.CopyToAsync(stream);
                }

                fileNames.Add(filename);
                section = await reader.ReadNextSectionAsync();
            }

            foreach (var file in fileNames)
            {
                await _fileRepository.Insert(new DAL.Model.File()
                {
                    Path = file,
                    UserId = _currentUserProvider.Id
                });
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
    }
}