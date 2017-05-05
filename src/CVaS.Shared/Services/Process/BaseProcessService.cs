using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CVaS.Shared.Services.File;
using CVaS.Shared.Services.Time;
using Microsoft.Extensions.Logging;

namespace CVaS.Shared.Services.Process
{
    /// <summary>
    /// Service that runs processes with given arguments
    /// Processing of process outputs is asynchronous and
    /// can be canceled
    /// </summary>
    public class BaseProcessService : IProcessService
    {
        private readonly FileSystemWrapper _fileSystemWrapper;
        private readonly ICurrentTimeProvider _currentTimeProvider;
        private readonly ILogger<BaseProcessService> _logger;

        public BaseProcessService(FileSystemWrapper fileSystemWrapper, ICurrentTimeProvider currentTimeProvider, ILogger<BaseProcessService> logger)
        {
            _fileSystemWrapper = fileSystemWrapper;
            _currentTimeProvider = currentTimeProvider;
            _logger = logger;
        }

        public async Task<ProcessResult> RunAsync(string filePath, IList<string> arguments,  CancellationToken cancellationToken)
        {
            return await RunAsync(filePath, arguments, _fileSystemWrapper.GetDirectoryFromFile(filePath), cancellationToken);
        }

        public async Task<ProcessResult> RunAsync(string filePath, IList<string> arguments, string workingDirectory, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<ProcessResult>();

            System.Diagnostics.Process process = new System.Diagnostics.Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    Arguments = String.Join(" ", arguments),
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
                result.FinishedAt = process.ExitTime;
                // result.FinishedAt = _currentTimeProvider.Now();
                tcs.TrySetResult(result);
                process.Dispose();
            };

            using (cancellationToken.Register(CanceledAction(tcs, process)))
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogInformation($"Launching process: {process.StartInfo.FileName} working directory: {process.StartInfo.WorkingDirectory}");
                if (process.Start() == false)
                {
                    tcs.TrySetException(new InvalidOperationException("Failed to start process"));
                }

                result.StartedAt = process.StartTime;

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                return await tcs.Task;
            }
        }

        private static Action CanceledAction(TaskCompletionSource<ProcessResult> tcs, System.Diagnostics.Process process)
        {
            return () =>
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
            };
        }
    }
}
