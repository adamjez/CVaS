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

        public async Task<ProcessResult> RunAsync(ProcessOptions options, CancellationToken cancellationToken)
        {
            options.WorkingDirectory = _fileSystemWrapper.GetDirectoryFromFile(options.FilePath);
            
            var fileExt = Path.GetExtension(options.FilePath);
            var interpreter = _interpreterResolver.Resolve(fileExt);

            if (!string.IsNullOrEmpty(interpreter))
            {
                options.Arguments.Insert(0, options.FilePath);
                options.FilePath = interpreter;
            }

            return await _processService.RunAsync(options, cancellationToken);
        }
    }
}