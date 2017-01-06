﻿using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CVaS.BL.Facades;
using CVaS.BL.Providers;
using CVaS.BL.Services.File;
using CVaS.Web.Helpers;
using CVaS.Web.Models.FileViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace CVaS.Web.Controllers.Api
{
    [Route("[controller]")]
    public class FilesController : ApiController
    {
        private readonly ILogger<FilesController> _logger;
        private readonly TemporaryFileProvider _fileProvider;
        private readonly FileFacade _fileFacade;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly RunFacade _runFacade;

        public FilesController(ILogger<FilesController> logger, TemporaryFileProvider fileProvider, FileFacade fileFacade,
            ICurrentUserProvider currentUserProvider, RunFacade runFacade)
        {
            _logger = logger;
            _fileProvider = fileProvider;
            _fileFacade = fileFacade;
            _currentUserProvider = currentUserProvider;
            _runFacade = runFacade;
        }

        /// <summary>
        /// Upload multiple files with multipart-form/data
        /// Files have to have filename with extenion defined
        /// </summary>
        /// <returns>Returns files identifier</returns>
        [HttpPost("")]
        public async Task<IActionResult> UploadMultipleFiles()
        {
            if (!MultipartFormHelpers.IsMultipartContentType(HttpContext.Request.ContentType))
            {
                return BadRequest();
            }

            var boundary = MultipartFormHelpers.GetBoundary(HttpContext.Request.ContentType);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);

            var pathFiles = new List<string>();
            MultipartSection section;
            while ((section = await reader.ReadNextSectionAsync()) != null)
            {
                var fileExtension = MultipartFormHelpers.GetExtension(section.ContentDisposition);
                if (string.IsNullOrEmpty(fileExtension))
                    continue;

                string filePath;
                using (var stream = _fileProvider.CreateTemporaryFile(fileExtension, out filePath))
                {
                    await section.Body.CopyToAsync(stream);
                }

                pathFiles.Add(filePath);
            }

            var files = await _fileFacade.AddUploadedAsync(pathFiles, _currentUserProvider.Id);

            return Ok(new UploadFilesResult { Ids = files.Select(file => file.Id) });
        }

        /// <summary>
        /// Retrieve file with given id.
        /// </summary>
        /// <param name="fileId">Identifier of file</param>
        /// <returns></returns>
        [HttpGet, Route("{fileId}", Name = nameof(GetFile))]
        public async Task GetFile(int fileId)
        {
            const string zipMime = "application/zip";

            var pathToFile = await _fileFacade.GetSafelyAsync(fileId);

            IFileInfo fileInfo = new PhysicalFileInfo(new FileInfo(pathToFile));

            HttpContext.Response.ContentType = zipMime;
            var service = HttpContext.Features.Get<IHttpSendFileFeature>();
            if (service != null)
            {
                await service.SendFileAsync(pathToFile, 0, fileInfo.Length, CancellationToken.None);
            }
            else
            {
                await HttpContext.Response.SendFileAsync(fileInfo);

            }
        }

        /// <summary>
        /// Deletes file with given file Id.
        /// </summary>
        /// <param name="fileId">File Identifier</param>
        /// <returns></returns>
        [HttpDelete, Route("{fileId}")]
        public async Task DeleteUserFile(int fileId)
        {
            const string zipMime = "application/zip";
            HttpContext.Response.ContentType = zipMime;

            await _fileFacade.DeleteAsync(fileId, _currentUserProvider.Id);
        }
    }
}