using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CVaS.BL.Services.Interpreter;
using CVaS.BL.Services.Process;

namespace CVaS.Web.Services
{
    public class WindowsDecoratorProcessService : IProcessService
    {
        private readonly IProcessService _processService;
        private readonly IInterpreterResolver _interpreterResolver;

        public WindowsDecoratorProcessService(IProcessService processService, IInterpreterResolver interpreterResolver)
        {
            _processService = processService;
            _interpreterResolver = interpreterResolver;
        }

        public async Task<ProcessResult> RunAsync(string filePath, IList<string> arguments, CancellationToken cancellationToken)
        {
            var fileExt = Path.GetExtension(filePath);

            var interpreter = _interpreterResolver.Resolve(fileExt);

            if (!string.IsNullOrEmpty(interpreter))
            {
                arguments.Insert(0, filePath);
                filePath = interpreter;
            }

            return await _processService.RunAsync(filePath, arguments, cancellationToken);
        }
    }
}