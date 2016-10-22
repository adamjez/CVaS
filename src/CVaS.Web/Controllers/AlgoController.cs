﻿using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CVaS.BL.Repositories;
using CVaS.BL.Services.Process;
using CVaS.Web.Models;
using CVaS.Web.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace CVaS.Web.Controllers
{
    [Route("[controller]")]
    public class AlgoController : ApiController
    {
        private readonly ILogger<AlgoController> logger;
        private readonly AlgorithmRepository repository;
        private readonly AlgorithmFileProvider fileProvider;
        private readonly IProcessService processService;

        public AlgoController(ILogger<AlgoController> logger, AlgorithmRepository repository,
            AlgorithmFileProvider fileProvider, IProcessService processService)
        {
            this.logger = logger;
            this.repository = repository;
            this.fileProvider = fileProvider;
            this.processService = processService;
        }

        [HttpPost("{codeName}")]
        public async Task<IActionResult> Process(string codeName, [FromBody] AlgorithmOptions options)
        {
            var algorithm = await repository.GetByCodeName(codeName);

            if (algorithm == null)
            {
                return NotFound("Given algorithm codeName doesn't exists");
            }
            
            var algoDir = fileProvider.GetAlgorithmDirectoryContents(codeName);
            if (algoDir == null)
            {
                return NotFound("Given algorithm execution file doesn't exists (1)");
            }

            var file = algoDir.FirstOrDefault(f => f.Name == algorithm.FilePath);

            if (file == null)
            {
                return NotFound($"Given algorithm execution file doesn't exists (2)");
            }


            var result = processService.Run(file.PhysicalPath, options.Arguments);

            return Ok(new AlgorithmResult
            {
                Title = algorithm.Title,
                Arguments = options.Arguments,
                StdOut = result.StdOut,
                StdError = result.StdError
            });
        }
    }
}