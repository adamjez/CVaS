using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CVaS.BL.Facades;
using CVaS.Web.Helpers;
using CVaS.Web.Models;
using Microsoft.Extensions.Logging;
using System.Linq;
using CVaS.Web.Providers;

namespace CVaS.Web.Controllers
{
    [Route("[controller]")]
    public class AlgoController : ApiController
    {
        private readonly ILogger<AlgoController> logger;
        private readonly RunFacade _runFacade;
        private readonly AlgoFacade _algoFacade;
        private readonly IEnumerable<IArgumentParserProvider> _argumentParserProviders;

        public AlgoController(ILogger<AlgoController> logger, RunFacade runFacade, AlgoFacade algoFacade, 
            IEnumerable<IArgumentParserProvider> argumentParserProviders)
        {
            this.logger = logger;
            _runFacade = runFacade;
            _algoFacade = algoFacade;
            _argumentParserProviders = argumentParserProviders;
        }

        [HttpPost("{codeName}")]
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

            return Ok(new AlgorithmResult
            {
                RunId = result.RunId,
                StdOut = result.StdOut,
                StdError = result.StdErr,
                Zip = result.FileName != null ? Url.Link(nameof(FilesController.GetResultZip), new { zipName = result.FileName }) : "<no-output>",
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
            var result = await _runFacade.RunHelpAsync(codeName);

            return Ok(result);
        }
    }
}