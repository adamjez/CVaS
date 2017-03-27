using System.Threading.Tasks;
using CVaS.DAL.Model;
using CVaS.Shared.Models;

namespace CVaS.Shared.Services.Launch
{
    public interface ILaunchService
    {
        Task<RunResult> LaunchAsync(Algorithm algorithm, Run run, RunSettings settings);
    }
}