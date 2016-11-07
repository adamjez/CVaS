using System;
using System.Collections.Generic;
using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using CVaS.BL.Repositories;
using CVaS.BL.Services.Process;
using CVaS.DAL.Model;
using CVaS.Web.Models;
using CVaS.Web.Services;
using Microsoft.Extensions.Logging;

namespace CVaS.Web.Controllers
{
    [Route("[controller]")]
    public class AlgoController : ApiController
    {
        private readonly ILogger<AlgoController> logger;
        private readonly AlgorithmRepository repository;
        private readonly FileProvider _fileProvider;
        private readonly AlgorithmFileProvider algFileProvider;
        private readonly IProcessService processService;
        private readonly FileRepository _fileRepository;
        private readonly TempFileProvider _fileSystemProvider;

        public AlgoController(ILogger<AlgoController> logger, AlgorithmRepository repository, FileProvider fileProvider,
            AlgorithmFileProvider algFileProvider, IProcessService processService, FileRepository fileRepository, TempFileProvider fileSystemProvider)
        {
            this.logger = logger;
            this.repository = repository;
            _fileProvider = fileProvider;
            this.algFileProvider = algFileProvider;
            this.processService = processService;
            _fileRepository = fileRepository;
            _fileSystemProvider = fileSystemProvider;
        }

        [HttpPost("{codeName}")]
        public async Task<IActionResult> Process(string codeName, [FromBody] AlgorithmOptions options)
        {
            var algorithm = await repository.GetByCodeName(codeName);

            if (algorithm == null)
            {
                return NotFound(Json("Given algorithm codeName doesn't exists"));
            }
            
            var filePath = algFileProvider.GetAlgorithmFilePath(codeName, algorithm.FilePath);
                        
            if (!_fileProvider.Exists(filePath))
            {
                return NotFound(Json("Given algorithm execution file doesn't exists"));
            }


            List<string> paths = new List<string>();
            foreach (var arg in options.Arguments)
            {
                var argEntity = await _fileRepository.GetById(int.Parse(arg.Content));
                paths.Add(argEntity.Path);
            }

            var runFolder = _fileSystemProvider.CreateTempFolder();

            var result = processService.Run(filePath, string.Join(" ", paths), runFolder);

            var zipPath = Guid.NewGuid() + ".zip";
            ZipFile.CreateFromDirectory(runFolder, zipPath, CompressionLevel.Fastest, false);

            return Ok(new AlgorithmResult
            {
                Title = algorithm.Title,
                //Arguments = options.Arguments,
                StdOut = result.StdOut,
                StdError = result.StdError,
                Zip = zipPath
            });
        }

        [HttpGet("{codeName}")]
        public async Task<IActionResult> RetrieveHelp(string codeName)
        {
            var algorithm = await repository.GetByCodeName(codeName);

            if (algorithm == null)
            {
                return NotFound(Json("Given algorithm codeName doesn't exists"));
            }

            return Ok(FormatHelp(algorithm));
        }

        public string FormatHelp(Algorithm algo)
        {
            return $"{algo.Title}\n" +
                   $"Description: {algo.Description}" +
                   $"CodeName: {algo.CodeName}" +
                   $"Arguments: {algo.Arguments.Select(x => x.Type)}";
        }
    }
}