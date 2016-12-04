using System.Collections.Generic;
using System.IO;
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

        public ProcessResult Run(string filePath, string workingDirectory, IList<string> arguments)
        {
            var fileExt = Path.GetExtension(filePath);

            if (fileExt == ".py")
            {
                arguments.Insert(0, filePath);
                filePath = @"C:\Users\adamj\AppData\Local\Programs\Python\Python35-32\python.exe";
            }

            return _processService.Run(filePath, workingDirectory, arguments);
        }
    }
}