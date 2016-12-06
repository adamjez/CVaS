using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CVaS.BL.Services.Process;

namespace CVaS.Web.Services
{
    public class WindowsDecoratorProcessService : IProcessService
    {
        private readonly IProcessService _processService;
        public WindowsDecoratorProcessService(IProcessService processService)
        {
            _processService = processService;
        }

        public async Task<ProcessResult> RunAsync(string filePath, string workingDirectory, IList<string> arguments, CancellationToken cancellationToken)
        {
            var fileExt = Path.GetExtension(filePath);

            if (fileExt == ".py")
            {
                arguments.Insert(0, filePath);
                filePath = @"C:\Users\adamj\AppData\Local\Programs\Python\Python35-32\python.exe";
            }

            return await _processService.RunAsync(filePath, workingDirectory, arguments, cancellationToken);
        }
    }
}