using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CVaS.BL.DTO;
using CVaS.BL.Facades;
using CVaS.BL.Services.Process;
using CVaS.DAL.Model;
using CVaS.Web.Models;
using CVaS.Web.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CVaS.Web.Controllers.Api
{
    [Route("[controller]")]
    public class AlgorithmsController : ApiController
    {
        private readonly ILogger<AlgorithmsController> logger;
        private readonly RunFacade _runFacade;
        private readonly AlgoFacade _algoFacade;
        private readonly IEnumerable<IArgumentParserProvider> _argumentParserProviders;

        public AlgorithmsController(ILogger<AlgorithmsController> logger, RunFacade runFacade, AlgoFacade algoFacade, 
            IEnumerable<IArgumentParserProvider> argumentParserProviders)
        {
            this.logger = logger;
            _runFacade = runFacade;
            _algoFacade = algoFacade;
            _argumentParserProviders = argumentParserProviders;
        }

        /// <summary>
        /// Start process with given algorithm name and with given options. Returns
        /// result of the process.
        /// </summary>
        /// <param name="codeName">CodeName of the algorithm to be run.</param>
        /// <param name="options">Options are translated to command-line arguments</param>
        [HttpPost("{codeName}")]
        [Produces(typeof(ProcessResult))]
        public async Task<IActionResult> Process(string codeName, [FromBody] object options)
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

            var result = await _runFacade.RunProcessAsync(codeName, parsedOptions);

            if (result.Result == RunResultType.NotFinished)
            {
                return Ok(new { result.RunId, result.Result });
            }

            return Ok(new AlgorithmResult
            {
                RunId = result.RunId,
                StdOut = result.StdOut,
                StdError = result.StdErr,
                Zip = result.FileName != null ? Url.Link(nameof(FilesController.GetFile), new { zipName = result.FileName }) : null,
                Result = result.Result,
                Duration = result.Duration
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