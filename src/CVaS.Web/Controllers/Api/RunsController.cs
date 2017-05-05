using System.Threading.Tasks;
using CVaS.BL.DTO;
using CVaS.BL.Facades;
using Microsoft.AspNetCore.Mvc;
using System;
using CVaS.BL.Providers;
using CVaS.Web.Models;

namespace CVaS.Web.Controllers.Api
{
    [Route("[controller]")]
    public class RunsController : ApiController
    {
        private readonly RunFacade _runFacade;
        private readonly ICurrentUserProvider _currentUserProvider;

        public RunsController(RunFacade runFacade, ICurrentUserProvider currentUserProvider)
        {
            _runFacade = runFacade;
            _currentUserProvider = currentUserProvider;
        }

        /// <summary>
        /// Retrieve basic informace about Run.
        /// </summary>
        /// <param name="runId">Identifier of run created by current user</param>
        /// <response code="200">Run retrieved</response>
        /// <response code="400">Run Id is missing/invalid</response>
        /// <response code="404">Run with given Id doesn't exist</response>
        [HttpGet("{runId}")]
        [ProducesResponseType(typeof(RunDTO), 200)]
        [ProducesResponseType(typeof(ApiError), 400)]
        [ProducesResponseType(typeof(ApiError), 404)]
        public async Task<IActionResult> Get(Guid runId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _runFacade.GetSafelyAsync(runId);
            result.File = result.FileId != null ? Url.Link(nameof(FilesController.GetFile), new { fileId = result.FileId }) : null;

            return Ok(result);
        }
    }
}