using CVaS.BL.Core.Provider;
using CVaS.DAL.Model;

namespace CVaS.BL.Repositories
{
    public class RunRepository : EntityFrameworkRepository<Run, int>
    {
        public RunRepository(IUnitOfWorkProvider provider) : base(provider)
        {
        }
    }
}