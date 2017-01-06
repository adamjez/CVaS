using System.Threading.Tasks;
using CVaS.BL.Facades;
using CVaS.BL.Providers;
using Microsoft.AspNetCore.Mvc;

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
        /// <param name="runId">Identifier of run paired with user</param>
        /// <returns></returns>
        [HttpGet("{runId}")]
        [Produces(typeof(DAL.Model.Run))]
        public async Task<IActionResult> Get(int runId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return Ok(await _runFacade.GetSafelyAsync(runId));
        }
    }
}