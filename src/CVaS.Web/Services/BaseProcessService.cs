﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CVaS.BL.Helpers;
using CVaS.BL.Services.File;
using CVaS.BL.Services.Process;

namespace CVaS.Web.Services
{
    /// <summary>
    /// Service that runs processes with given arguments
    /// Processing of process outputs is asynchronous and
    /// can be canceled
    /// </summary>
    public class BaseProcessService : IProcessService
    {
        private readonly FileProvider _fileProvider;
        private readonly ICurrentTimeProvider _currentTimeProvider;

        public BaseProcessService(FileProvider fileProvider, ICurrentTimeProvider currentTimeProvider)
        {
            _fileProvider = fileProvider;
            _currentTimeProvider = currentTimeProvider;
        }

        public async Task<ProcessResult> RunAsync(string filePath, IList<string> arguments,  CancellationToken cancellationToken)
        {
            return await RunAsync(filePath, arguments, _fileProvider.GetDirectoryFromFile(filePath), cancellationToken);
        }

        public async Task<ProcessResult> RunAsync(string filePath, IList<string> arguments, string workingDirectory, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<ProcessResult>();

            Process process = new Process()
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
                result.FinishedAt = _currentTimeProvider.Now();
                tcs.TrySetResult(result);
                process.Dispose();
            };

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

                result.StartedAt = _currentTimeProvider.Now();
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
