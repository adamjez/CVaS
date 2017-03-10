using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CVaS.Shared.Services.File;
using CVaS.Shared.Services.Interpreter;

namespace CVaS.Shared.Services.Process
{
    /// <summary>
    /// Decorator pattern for proces service to
    /// provide intepret for scripts, It's only
    /// needed for OS Windows bcs Linux-like OS
    /// can specify interpreter in Shebang
    /// </summary>
    public class WindowsDecoratorProcessService : IProcessService
    {
        private readonly IProcessService _processService;
        private readonly IInterpreterResolver _interpreterResolver;
        private readonly FileSystemWrapper _fileSystemWrapper;


        public WindowsDecoratorProcessService(IProcessService processService, IInterpreterResolver interpreterResolver, FileSystemWrapper fileSystemWrapper)
        {
            _processService = processService;
            _interpreterResolver = interpreterResolver;
            _fileSystemWrapper = fileSystemWrapper;
        }

        public async Task<ProcessResult> RunAsync(string filePath, IList<string> arguments, string workingDirectory, CancellationToken cancellationToken)
        {
            return await RunAsync(filePath, arguments, cancellationToken);
        }

        public async Task<ProcessResult> RunAsync(string filePath, IList<string> arguments, CancellationToken cancellationToken)
        {
            var fileExt = Path.GetExtension(filePath);
            var workingDirectory = _fileSystemWrapper.GetDirectoryFromFile(filePath);

            var interpreter = _interpreterResolver.Resolve(fileExt);

            if (!string.IsNullOrEmpty(interpreter))
            {
                arguments.Insert(0, filePath);
                filePath = interpreter;
            }

            return await _processService.RunAsync(filePath, arguments, workingDirectory, cancellationToken);
        }
    }
}