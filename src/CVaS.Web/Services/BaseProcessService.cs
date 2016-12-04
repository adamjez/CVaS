using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CVaS.BL.Services.File;
using CVaS.BL.Services.Process;

namespace CVaS.Web.Services
{
    public class BaseProcessService : IProcessService
    {
        private readonly FileProvider _fileProvider;
        public BaseProcessService(FileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public ProcessResult Run(string filePath, string workingDirectory, IList<string> arguments)
        {
            var result = new ProcessResult {StdError = "", StdOut = ""};
            var escapedArguments = String.Join(" ", arguments.Select(x => $"\"{x}\""));
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    Arguments = escapedArguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory
                }
            };

            process.Start();
            result.StdOut = process.StandardOutput.ReadToEnd();
            result.StdError = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return result;
        }
    }
}
