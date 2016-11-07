﻿using System;
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


            var runFolder = _fileSystemProvider.CreateTempFolder();

            var result = processService.Run(filePath, runFolder, args);

            var zipPath = "<output empty>";
            if (!_fileProvider.IsEmpty(runFolder))
            {
                zipPath = Guid.NewGuid() + ".zip";
                ZipFile.CreateFromDirectory(runFolder, zipPath, CompressionLevel.Fastest, false);
            }

            return Ok(new AlgorithmResult
            {
                Title = algorithm.Title,
                StdOut = result.StdOut,
                StdError = result.StdError,
                Zip = zipPath,
                RunFolder = runFolder
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