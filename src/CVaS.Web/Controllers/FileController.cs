﻿using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CVaS.BL.Facades;
using CVaS.BL.Providers;
using CVaS.BL.Repositories;
using CVaS.BL.Services.File;
using CVaS.DAL;
using CVaS.Web.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Logging;

namespace CVaS.Web.Controllers
{
    [Route("[controller]")]
    public class FileController : ApiController
    {
        private readonly ILogger<FileController> _logger;
        private readonly TemporaryFileProvider _fileProvider;
        private readonly FileRepository _fileRepository;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly AppDbContext _context;
        private readonly TemporaryFileProvider _temporaryFileProvider;
        private readonly RunFacade _runFacade;

        public FileController(ILogger<FileController> logger, TemporaryFileProvider fileProvider, FileRepository fileRepository,
            ICurrentUserProvider currentUserProvider, AppDbContext context, TemporaryFileProvider temporaryFileProvider, RunFacade runFacade)
        {
            _logger = logger;
            this._fileProvider = fileProvider;
            this._fileRepository = fileRepository;
            _currentUserProvider = currentUserProvider;
            _context = context;
            _temporaryFileProvider = temporaryFileProvider;
            _runFacade = runFacade;
        }

        [HttpPost("")]
        public async Task<IActionResult> UploadMultipleFiles()
        {
            if (!MultipartFormHelpers.IsMultipartContentType(HttpContext.Request.ContentType))
            {
                return BadRequest();
            }

            var boundary = MultipartFormHelpers.GetBoundary(HttpContext.Request.ContentType);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);

            var files = new List<DAL.Model.File>();
            MultipartSection section;
            while ((section = await reader.ReadNextSectionAsync()) != null)
            {
                string filePath;
                using (var stream = _fileProvider.CreateTemporaryFile(
                    MultipartFormHelpers.GetExtension(section.ContentDisposition), 
                    out filePath))
                {
                    await section.Body.CopyToAsync(stream);
                }

                files.Add(new DAL.Model.File()
                {
                    Path = filePath,
                    UserId = _currentUserProvider.Id
                });
            }

            _context.Files.AddRange(files);
            await _context.SaveChangesAsync();

            return Ok(files);
        }

        [HttpGet, Route("{runId}", Name = nameof(GetResultZip))]
        public async Task GetResultZip(int runId)
        {
            const string zipMime = "application/zip";
            HttpContext.Response.ContentType = zipMime;

            var service = HttpContext.Features.Get<IHttpSendFileFeature>();

            var run = await _runFacade.GetSafelyAsync(runId);

            var pathToFile = _temporaryFileProvider.ResolveTemporaryFilePath(run.Path);
            IFileInfo fileInfo = new PhysicalFileInfo(new FileInfo(pathToFile));

            if (service != null)
            {
                await service.SendFileAsync(pathToFile, 0, fileInfo.Length, CancellationToken.None);
            }
            else
            {
                await HttpContext.Response.SendFileAsync(fileInfo);

            }
        }
    }
}