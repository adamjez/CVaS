using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CVaS.BL.Facades;
using CVaS.BL.Repositories;
using CVaS.Web.Models;
using Microsoft.Extensions.Logging;

namespace CVaS.Web.Controllers
{
    [Route("[controller]")]
    public class AlgoController : ApiController
    {
        private readonly ILogger<AlgoController> logger;
        private readonly AlgorithmRepository repository;
        private readonly RunFacade _runFacade;
        private readonly AlgoFacade _algoFacade;

        public AlgoController(ILogger<AlgoController> logger, AlgorithmRepository repository, RunFacade runFacade, AlgoFacade algoFacade)
        {
            this.logger = logger;
            this.repository = repository;
            _runFacade = runFacade;
            _algoFacade = algoFacade;
        }

        [HttpPost("{codeName}")]
        public async Task<IActionResult> Process(string codeName, [FromBody] AlgorithmOptions options)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _runFacade.RunProcessAsync(codeName, options.Arguments);

            return Ok(new AlgorithmResult
            {
                RunId = result.RunId,
                StdOut = result.StdOut,
                StdError = result.StdErr,
                Zip = result.FileName != null ? Url.Link(nameof(FileController.GetResultZip), new { zipName = result.FileName }) : "<no-output>",
                Result = result.Result
            });
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _algoFacade.GetAll());
        }

        [HttpGet, Route("{codeName}")]
        public async Task<IActionResult> RetrieveHelp(string codeName)
        {
            var algorithm = await repository.GetByCodeNameWithArgs(codeName);

            if (algorithm == null)
            {
                return NotFound(Json("Given algorithm codeName doesn't exists"));
            }

            return Ok(algorithm);
        }
    }
}