using System;
using System.Linq;
using System.Collections.Generic;
using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CVaS.BL.Repositories;
using CVaS.BL.Services.Process;
using CVaS.DAL;
using CVaS.DAL.Model;
using CVaS.Web.Models;
using CVaS.Web.Services;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

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
        private readonly TemporaryFileProvider _fileSystemProvider;
        private readonly AppDbContext _context;

        public AlgoController(ILogger<AlgoController> logger, AlgorithmRepository repository, FileProvider fileProvider,
            AlgorithmFileProvider algFileProvider, IProcessService processService, FileRepository fileRepository, 
            TemporaryFileProvider fileSystemProvider, AppDbContext context)
        {
            this.logger = logger;
            this.repository = repository;
            _fileProvider = fileProvider;
            this.algFileProvider = algFileProvider;
            this.processService = processService;
            _fileRepository = fileRepository;
            _fileSystemProvider = fileSystemProvider;
            _context = context;
        }

        [HttpPost("{codeName}")]
        public async Task<IActionResult> Process(string codeName, [FromBody] AlgorithmOptions options)
        {
            var algorithm = await repository.GetByCodeNameWithArgs(codeName);

            if (algorithm == null)
            {
                return NotFound(Json("Given algorithm codeName doesn't exists"));
            }

            var filePath = algFileProvider.GetAlgorithmFilePath(codeName, algorithm.FilePath);

            if (!_fileProvider.Exists(filePath))
            {
                return NotFound(Json("Given algorithm execution file doesn't exists" + filePath));
            }


            List<string> args = new List<string>();
            if (options.Arguments != null)
            {
                foreach (var arg in options.Arguments)
                {
                    if (arg.Type == ArgumentType.File)
                    {
                        // Check for user
                        var argEntity = await _fileRepository.GetById(int.Parse(arg.Content));
                        args.Add(argEntity.Path);
                    }
                    else if (arg.Type == ArgumentType.Number)
                    {
                        // Validate?
                        args.Add(arg.Content);
                    }
                }
            }


            var runFolder = _fileSystemProvider.CreateTemporaryFolder();

            var result = processService.Run(filePath, runFolder, args);

            BasicFileInfo zipFile = null;
            if (!_fileProvider.IsEmpty(runFolder))
            {
                zipFile = _fileSystemProvider.GetTemporaryFile();
                ZipFile.CreateFromDirectory(runFolder, zipFile.FullPath, CompressionLevel.Fastest, false);
            }


            return Ok(new AlgorithmResult
            {
                Title = algorithm.Title,
                StdOut = result.StdOut,
                StdError = result.StdError,
                Zip = zipFile != null ? Url.Link(nameof(FileController.GetResultZip), new { zipName = zipFile.FileName }) : "<no-output>"
            });
        }

        [HttpGet("")]
        public IActionResult GetAll()
        {
            var algorithms = _context.Algorithms.Include(x => x.Arguments).ToList();

            return Ok(algorithms);
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