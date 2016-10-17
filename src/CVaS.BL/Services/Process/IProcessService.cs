namespace CVaS.BL.Services.Process
{
    public interface IProcessService
    {
        string Run(string filePath, string arguments);
    }
}
