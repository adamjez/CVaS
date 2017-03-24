using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;
using System;

namespace CVaS.Shared.Repositories
{
    public class RunRepository : EntityFrameworkRepository<Run, Guid>
    {
        public RunRepository(IUnitOfWorkProvider provider) : base(provider)
        {
        }
    }
}