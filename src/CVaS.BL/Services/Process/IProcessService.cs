namespace CVaS.BL.Services.Process
{
    public interface IProcessService
    {
        ProcessResult Run(string filePath, string arguments);
    }

    public class ProcessResult
    {
        public string StdOut { get; set; }
        public string StdError { get; set; }
    }
}
