using System.Collections.Generic;

namespace CVaS.Shared.Services.Process
{
    public struct ProcessOptions
    {
        public string FilePath { get; set; }
        public IList<string> Arguments { get; set; }
        public string WorkingDirectory { get; set; }
        public string DestinationDirectory { get; set; }
        
        public ProcessOptions(string filePath, IList<string> arguments, 
            string destinationDirectory, string workingDirectory = null)
        {
            FilePath = filePath;
            Arguments = arguments;
            WorkingDirectory = workingDirectory;
            DestinationDirectory = destinationDirectory;
        }
    }
}
