using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task<ProcessResult> RunAsync(string filePath, string workingDirectory, IList<string> arguments, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<ProcessResult>();

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
                },
                EnableRaisingEvents = true
            };
            var result = new ProcessResult { StdError = "", StdOut = "" };

            process.OutputDataReceived += (sender, args) =>
            {
                result.StdOut += args.Data;
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                result.StdError += args.Data;
            };

            process.Exited += (sender, args) =>
            {
                result.ExitCode = process.ExitCode;
                tcs.TrySetResult(result);
                process.Dispose();
            };

            //process.Start();

            //process.BeginOutputReadLine();
            //process.BeginErrorReadLine();
            ////result.StdOut = process.StandardOutput.ReadToEnd();
            ////result.StdError = process.StandardError.ReadToEnd();
            ////process.WaitForExit();

            //return await tcs.Task;
            using (cancellationToken.Register(() =>
            {
                tcs.TrySetCanceled();

                try
                {
                    if (!process.HasExited)

                    {
                        process.Kill();
                    }
                }
                catch (InvalidOperationException) { }
            }))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (process.Start() == false)
                {
                    tcs.TrySetException(new InvalidOperationException("Failed to start process"));
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                return await tcs.Task;
            }

        }
    }
}
