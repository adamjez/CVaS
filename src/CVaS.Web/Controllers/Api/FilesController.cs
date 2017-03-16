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
using CVaS.Shared.Providers;
using CVaS.Shared.Services.File.Providers;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace CVaS.Web.Controllers.Api
{
    [Route("[controller]")]
    public class FilesController : ApiController
    {
        private readonly ILogger<FilesController> _logger;
        private readonly FileFacade _fileFacade;
        private readonly IUserFileProvider _userFileProvider;

        public FilesController(ILogger<FilesController> logger, FileFacade fileFacade, RunFacade runFacade, IUserFileProvider userFileProvider)
        {
            _logger = logger;
            _fileFacade = fileFacade;
            _userFileProvider = userFileProvider;
        }

        /// <summary>
        /// Upload multiple files with multipart-form/data
        /// Files have to have filename with extenion defined.
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
            if (boundary == null)
            {
                return BadRequest("Missing Boundary");
            }
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);

            var fileIds = new List<int>();
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
        public async Task<FileStreamResult> GetFile(int fileId)
        {
            if (!ModelState.IsValid)
            {
                throw new BadRequestException("File Id has to  be specified");
            }

            var remoteFileId = await _fileFacade.GetSafelyAsync(fileId);

            if (_userFileProvider is UserSystemFileProvider)
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
                var result = await _userFileProvider.GetAsync(remoteFileId);

                return new FileStreamResult(result.Content, result.ContentType);
            }

            return null;
        }

        /// <summary>
        /// Deletes file with given file Id.
        /// </summary>
        /// <param name="fileId">File Identifier</param>
        [HttpDelete, Route("{fileId}")]
        public async Task<IActionResult> DeleteUserFile(int fileId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("File Id has to  be specified");
            }

            await _fileFacade.DeleteAsync(fileId);
            return Ok();
        }

        private string GetContentType(string fileName)
        {
            string contentType;
            return new FileExtensionContentTypeProvider()
                .TryGetContentType(fileName, out contentType) ? contentType : "application/octet-stream";
        }
    }
}