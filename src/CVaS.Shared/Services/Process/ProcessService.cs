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
    public class ProcessService : IProcessService
    {
        private const string DestinationDirectoryKey = "DESTINATION_DIRECTORY";
        private readonly FileSystemWrapper _fileSystemWrapper;
        private readonly ICurrentTimeProvider _currentTimeProvider;
        private readonly ILogger<ProcessService> _logger;

        public ProcessService(FileSystemWrapper fileSystemWrapper, ICurrentTimeProvider currentTimeProvider, ILogger<ProcessService> logger)
        {
            _fileSystemWrapper = fileSystemWrapper;
            _currentTimeProvider = currentTimeProvider;
            _logger = logger;
        }

        public async Task<ProcessResult> RunAsync(ProcessOptions options, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<ProcessResult>();

            System.Diagnostics.Process process = new System.Diagnostics.Process()
            {
                StartInfo = CreateProcessStartInfo(options),
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
                result.FinishedAt = _currentTimeProvider.Now;
                tcs.TrySetResult(result);
                process.Dispose();
            };

            using (cancellationToken.Register(CanceledAction(tcs, process)))
            {
                cancellationToken.ThrowIfCancellationRequested();

                result.StartedAt = _currentTimeProvider.Now;


                _logger.LogInformation($"Launching process: {process.StartInfo.FileName} working directory: {process.StartInfo.WorkingDirectory}");
                if (process.Start() == false)
                {
                    tcs.TrySetException(new InvalidOperationException("Failed to start process"));
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                return await tcs.Task;
            }
        }

        private ProcessStartInfo CreateProcessStartInfo(ProcessOptions options)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = options.FilePath,
                Arguments = String.Join(" ", options.Arguments),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = options.WorkingDirectory 
                    ?? _fileSystemWrapper.GetDirectoryFromFile(options.FilePath)
            };
            startInfo.Environment.Add(DestinationDirectoryKey, options.DestinationDirectory);
            return startInfo;
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
                catch (System.ComponentModel.Win32Exception) { }
            };
        }
    }
}
