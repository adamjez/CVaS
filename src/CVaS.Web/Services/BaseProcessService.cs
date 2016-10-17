using System.Diagnostics;
using System.Text;
using CVaS.BL.Services.Process;

namespace CVaS.Web.Services
{
    public class BaseProcessService : IProcessService
    {
        public string Run(string filePath, string arguments)
        {
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
            while (!process.StandardOutput.EndOfStream)
            {
                buffer.AppendLine(process.StandardOutput.ReadLine());
            }

            return buffer.ToString();
        }
    }
}
