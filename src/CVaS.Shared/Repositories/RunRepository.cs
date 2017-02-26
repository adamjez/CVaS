using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;

namespace CVaS.Shared.Repositories
{
    public class RunRepository : EntityFrameworkRepository<Run, int>
    {
        public RunRepository(IUnitOfWorkProvider provider) : base(provider)
        {
        }
    }
}