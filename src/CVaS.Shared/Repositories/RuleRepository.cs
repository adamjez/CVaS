using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;

namespace CVaS.Shared.Repositories
{
    public class RuleRepository : EntityFrameworkRepository<Rule, int>
    {
        public RuleRepository(IUnitOfWorkProvider provider) : base(provider)
        {
        }
    }
}