using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CVaS.BL.Repositories;
using CVaS.BL.Services.Process;
using CVaS.DAL.Model;
using CVaS.Web.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace CVaS.Web.Controllers
{
    [Route("[controller]")]
    public class AlgoController : ApiController
    {
        private readonly ILogger<AlgoController> logger;
        private readonly AlgorithmRepository repository;
        private readonly IFileProvider fileProvider;
        private readonly IProcessService processService;

        public AlgoController(ILogger<AlgoController> logger, AlgorithmRepository repository, IFileProvider fileProvider, IProcessService processService)
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

            var algoDir = fileProvider.GetDirectoryContents("Algorithms");
            var file = algoDir.FirstOrDefault(f => f.Name == algorithm.FilePath);

            if (file == null)
            {
                return NotFound("Given algorithm execution file doesn't exists");
            }

            var result = processService.Run(file.PhysicalPath, options.Arguments);

            return Ok(new AlgorithmResult
            {
                Title = algorithm.Title,
                Arguments = options.Arguments,
                Result = result
            });
        }

        //[HttpGet("{program}/{arguments}")]
        //public IActionResult Get(string program, string arguments)
        //{
            

        //    Process process = new Process()
        //    {
        //        StartInfo = new ProcessStartInfo
        //        {
        //            FileName = fileName.PhysicalPath,
        //            Arguments = arguments,
        //            UseShellExecute = false,
        //            RedirectStandardOutput = true,
        //            CreateNoWindow = true
        //        }
        //    };

        //    logger.LogInformation("Starting Program");
        //    process.Start();
        //    var buffer = new StringBuilder();
        //    while (!process.StandardOutput.EndOfStream)
        //    {
        //        buffer.AppendLine(process.StandardOutput.ReadLine());
        //    }
        //    logger.LogInformation("Ending Program");


        //    return Ok(buffer.ToString());
        //}
        

    }
}