using System.Diagnostics;
using System.Text;
using CVaS.BL.Services.Process;

namespace CVaS.Web.Services
{
    public class BaseProcessService : IProcessService
    {
        public ProcessResult Run(string filePath, string arguments)
        {
            var result = new ProcessResult {StdError = "", StdOut = ""};
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var buffer = new StringBuilder();
            var bufferError = new StringBuilder();
            while (!process.StandardOutput.EndOfStream)
            {
                buffer.AppendLine(process.StandardOutput.ReadLine());
                bufferError.AppendLine(process.StandardError.ReadLine());
            }

            result.StdOut = buffer.ToString();
            result.StdError = bufferError.ToString();

            return result;
        }
    }
}
