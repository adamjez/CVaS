using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CVaS.BL.DTO;
using CVaS.BL.Facades;
using CVaS.Shared.Services.Process;
using CVaS.Web.Controllers.Web;
using CVaS.Web.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AspNet.Mvc.TypedRouting.LinkGeneration;

namespace CVaS.Web.Controllers.Api
{
    [Route("[controller]")]
    public class AlgorithmsController : ApiController
    {
        private readonly ILogger<AlgorithmsController> _logger;
        private readonly RunFacade _runFacade;
        private readonly AlgorithmFacade _algoFacade;
        private readonly IEnumerable<IArgumentParser> _argumentParserProviders;

        public AlgorithmsController(ILogger<AlgorithmsController> logger, RunFacade runFacade, AlgorithmFacade algoFacade,
            IEnumerable<IArgumentParser> argumentParserProviders)
        {
            _logger = logger;
            _runFacade = runFacade;
            _algoFacade = algoFacade;
            _argumentParserProviders = argumentParserProviders;
        }

        /// <summary>
        /// Start process with given algorithm name and with given options. Returns
        /// result of the process.
        /// </summary>
        /// <param name="codeName">CodeName of the algorithm to be run.</param>
        /// <param name="options">Options are translated to command-line arguments. Expected
        /// format is json containing one element or single array with simple types or dictionary with
        /// only simple types</param>
        /// <param name="timeout">Specifies maximum amount of time to wait for returning request. If timeout
        /// is set to negative number, wait time is set to maximum possible time.</param>
        [HttpPost("{codeName}")]
        [Produces(typeof(RunDTO))]
        public async Task<IActionResult> Process(string codeName, [FromBody] object options, [FromQuery] int? timeout)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var parser = _argumentParserProviders.FirstOrDefault(p => p.CanParse(options));

            if (parser == null)
            {
                return new UnsupportedMediaTypeResult();
            }

            List<object> parsedOptions;
            try
            {
                parsedOptions = parser.Parse(options);
            }
            catch (Exception)
            {
                ModelState.AddModelError(nameof(options), "Arguments are in bad format. Look at documentation");
                return BadRequest(ModelState);
            }

            var result = await _runFacade.RunAlgorithmAsync(codeName, parsedOptions, timeout);

            return this.CreatedAtAction<RunsController>(c => c.Get(With.No<Guid>()), new { runId = result.RunId },
                new RunDTO
                {
                    Id = result.RunId,
                    StdOut = result.StdOut,
                    StdErr = result.StdErr,
                    Zip = result.FileId != null ? Url.Link(nameof(FilesController.GetFile), new { fileId = result.FileId }) : null,
                    Status = result.Result,
                    CreatedAt = result.CreatedAt,
                    FinishedAt = result.FinishedAt
                });
        }

        /// <summary>
        /// Retrieve all algortihms hosted on server.
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [Produces(typeof(IEnumerable<AlgorithmListDTO>))]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _algoFacade.GetAll());
        }

        /// <summary>
        /// Retrieve help information about given algorithm.
        /// </summary>
        /// <param name="codeName">CodeName for algorithm</param>
        /// <returns></returns>
        [HttpGet("{codeName}")]
        [Produces(typeof(ProcessResult))]
        public async Task<IActionResult> RetrieveHelp(string codeName)
        {
            var result = await _runFacade.RunHelpAsync(codeName);

            return Ok(result);
        }
    }
}