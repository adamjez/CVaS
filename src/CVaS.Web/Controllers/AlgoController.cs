using System;
using System.Collections.Generic;
using System.IO.Compression;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using CVaS.BL.Repositories;
using CVaS.BL.Services.Process;
using CVaS.DAL;
using CVaS.DAL.Model;
using CVaS.Web.Models;
using CVaS.Web.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
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
        private readonly IUrlHelper _urlHelper;

        public AlgoController(ILogger<AlgoController> logger, AlgorithmRepository repository, FileProvider fileProvider,
            AlgorithmFileProvider algFileProvider, IProcessService processService, FileRepository fileRepository, 
            TemporaryFileProvider fileSystemProvider, AppDbContext context, IUrlHelperFactory urlHelperFactory, 
            IActionContextAccessor actionContextAccessor)
        {
            this.logger = logger;
            this.repository = repository;
            _fileProvider = fileProvider;
            this.algFileProvider = algFileProvider;
            this.processService = processService;
            _fileRepository = fileRepository;
            _fileSystemProvider = fileSystemProvider;
            _context = context;
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
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

            string zipPath = null;
            if (!_fileProvider.IsEmpty(runFolder))
            {
                zipPath = Guid.NewGuid() + ".zip";
                ZipFile.CreateFromDirectory(runFolder, zipPath, CompressionLevel.Fastest, false);
            }


            var controllerName = nameof(FileController);
            return Ok(new AlgorithmResult
            {
                Title = algorithm.Title,
                StdOut = result.StdOut,
                StdError = result.StdError,
                Zip = zipPath != null ?_urlHelper.Action(nameof(FileController.GetResultZip), controllerName, new {zipName = zipPath})
                        :   "<no-output>"
            });
        }

        [HttpGet("")]
        public IActionResult GetAll()
        {
            var algorithms = _context.Algorithms.Include(x => x.Arguments).ToList();

            return Ok(algorithms);
        }

        [HttpGet("{codeName}")]
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