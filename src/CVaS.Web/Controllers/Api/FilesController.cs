using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CVaS.BL.Facades;
using CVaS.Web.Helpers;
using CVaS.Web.Models.FileViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using CVaS.Shared.Exceptions;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using System;
using CVaS.BL.DTO;
using CVaS.Shared.Services.File.User;
using Microsoft.AspNetCore.Http.Extensions;
using CVaS.Web.Swagger;
using Microsoft.AspNetCore.Http.Internal;

namespace CVaS.Web.Controllers.Api
{
    [Route("[controller]")]
    public class FilesController : ApiController
    {
        private readonly ILogger<FilesController> _logger;
        private readonly FileFacade _fileFacade;
        private readonly IFileStorage _fileStorage;

        public FilesController(ILogger<FilesController> logger, FileFacade fileFacade, RunFacade runFacade, IFileStorage fileStorage)
        {
            _logger = logger;
            _fileFacade = fileFacade;
            _fileStorage = fileStorage;
        }

        /// <summary>
        /// Retrieves all user files metainformations
        /// </summary>
        [HttpGet, Route(""), Produces(typeof(IEnumerable<FileDTO>))]
        public async Task<IEnumerable<FileDTO>> GetUserFiles()
        {
            return await _fileFacade.GetAllBySignedUserAsync();
        }

        /// <summary>
        /// Upload multiple files with multipart-form/data
        /// Files have to have filename with extenion defined.
        /// </summary>
        /// <returns>Returns files identifier</returns>
        [HttpPost("")]
        [FileParams("file", description: "File to upload.")]
        public async Task<IActionResult> UploadMultipleFiles()
        {
            if (!HttpContext.Request.IsMultipartContentType())
            {
                return BadRequest();
            }

            var boundary = HttpContext.Request.GetMultipartBoundary();
            if (string.IsNullOrEmpty(boundary))
            {
                return BadRequest("Missing Boundary");
            }

            var reader = new MultipartReader(boundary, HttpContext.Request.Body);

            var fileIds = new List<Guid>();
            MultipartSection section;
            while ((section = await reader.ReadNextSectionAsync()) != null)
            {
                var fileName = MultipartFormHelpers.GetFileName(section.ContentDisposition);
                if (string.IsNullOrEmpty(fileName))
                    continue;

                var id = await _fileFacade.AddFileAsync(section.Body, fileName, GetContentType(fileName));

                fileIds.Add(id);
            }

            return Ok(new UploadFilesResult { Ids = fileIds });
        }

        /// <summary>
        /// Retrieve file with given id.
        /// </summary>
        /// <param name="fileId">Identifier of file</param>
        [HttpGet, Route("{fileId}", Name = nameof(GetFile))]
        public async Task<FileStreamResult> GetFile(Guid fileId)
        {
            if (!ModelState.IsValid)
            {
                throw new BadRequestException("File Id has to  be specified");
            }

            var remoteFileId = (await _fileFacade.GetSafelyAsync(fileId)).LocationId;

            if (_fileStorage is FileSystemStorage)
            {
                IFileInfo fileInfo = new PhysicalFileInfo(new FileInfo(remoteFileId));

                HttpContext.Response.ContentType = GetContentType(fileInfo.Name);

                var service = HttpContext.Features.Get<IHttpSendFileFeature>();
                if (service != null)
                {
                    await service.SendFileAsync(remoteFileId, 0, fileInfo.Length, CancellationToken.None);
                }
                else
                {
                    await HttpContext.Response.SendFileAsync(fileInfo);

                }
            }
            else
            {
                var result = await _fileStorage.GetAsync(remoteFileId);

                return new FileStreamResult(result.Content, result.ContentType);
            }

            return null;
        }

        /// <summary>
        /// Deletes file with given file Id.
        /// </summary>
        /// <param name="fileId">File Identifier</param>
        [HttpDelete, Route("{fileId}")]
        public async Task<IActionResult> DeleteUserFile(Guid fileId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("File Id has to  be specified");
            }

            await _fileFacade.DeleteAsync(fileId);
            return Ok();
        }

        /// <summary>
        /// Retrieve meta information about file with given file Id.
        /// Currently returns only Content-Type of file
        /// </summary>
        /// <param name="fileId">File Identifier</param>
        [HttpHead, Route("{fileId}")]
        public async Task<IActionResult> HeadUserFile(Guid fileId)
        {
            if (!ModelState.IsValid)
            {
                throw new BadRequestException("File Id has to  be specified");
            }

            var file = await _fileFacade.GetSafelyAsync(fileId);

            HttpContext.Response.ContentType = file.ContentType;
            HttpContext.Response.ContentLength = file.FileSize;

            return Ok();
        }

        private string GetContentType(string fileName)
        {
            return new FileExtensionContentTypeProvider()
                .TryGetContentType(fileName, out string contentType) ? contentType : "application/octet-stream";
        }
    }
}