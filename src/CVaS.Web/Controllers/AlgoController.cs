using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using CVaS.BL.Repositories;
using CVaS.DAL.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace CVaS.Web.Controllers
{
    [Route("[controller]")]
    public class AlgoController : Controller
    {
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly ILogger<AlgoController> logger;
        private readonly AlgorithmRepository repository;

        public AlgoController(IHostingEnvironment hostingEnvironment, ILogger<AlgoController> logger, AlgorithmRepository repository)
        {
            this.hostingEnvironment = hostingEnvironment;
            this.logger = logger;
            this.repository = repository;
        }

        [HttpGet("")]
        public IActionResult GetTest()
        {
            repository.Insert(new Algorithm { Title = "Mega Giga Super"});

            return Ok("test");
        }

        [HttpGet("{program}/{arguments}")]
        public IActionResult Get(string program, string arguments)
        {
            var algFolderPath = hostingEnvironment.ContentRootPath + "\\Algorithms\\" ;
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = algFolderPath + program,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            logger.LogInformation("Starting Program");
            process.Start();
            var buffer = new StringBuilder();
            while (!process.StandardOutput.EndOfStream)
            {
                buffer.AppendLine(process.StandardOutput.ReadLine());
            }
            logger.LogInformation("Ending Program");


            return Ok(buffer.ToString());
        }
        

    }
}