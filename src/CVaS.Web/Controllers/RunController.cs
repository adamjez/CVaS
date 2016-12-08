using System.Threading.Tasks;
using CVaS.BL.Facades;
using CVaS.BL.Providers;
using Microsoft.AspNetCore.Mvc;

namespace CVaS.Web.Controllers
{
    [Route("[controller]")]
    public class RunController : ApiController
    {
        private readonly RunFacade _runFacade;
        private readonly ICurrentUserProvider _currentUserProvider;

        public RunController(RunFacade runFacade, ICurrentUserProvider currentUserProvider)
        {
            _runFacade = runFacade;
            _currentUserProvider = currentUserProvider;
        }

        [HttpGet("{runId}")]
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