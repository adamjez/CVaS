using System.Collections.Generic;
using System.Threading.Tasks;
using CVaS.DAL.Model;
using CVaS.Shared.Models;

namespace CVaS.Shared.Services.Launch
{
    public interface ILaunchService
    {
        Task<RunResult> LaunchAsync(string codeName, string pathFile, List<Argument.Argument> args, Run run);
    }
}