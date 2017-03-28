using System.Threading.Tasks;
using CVaS.BL.DTO;
using CVaS.BL.Facades;
using Microsoft.AspNetCore.Mvc;
using System;
using CVaS.BL.Providers;

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
        /// <returns></returns>
        [HttpGet("{runId}")]
        [Produces(typeof(RunDTO))]
        public async Task<IActionResult> Get(Guid runId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _runFacade.GetSafelyAsync(runId);
            result.Zip = result.FileId != null ? Url.Link(nameof(FilesController.GetFile), new { fileId = result.FileId }) : null;

            return Ok(result);
        }
    }
}