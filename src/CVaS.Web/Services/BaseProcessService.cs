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
                    RedirectStandardError = true,
                    CreateNoWindow = true
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
