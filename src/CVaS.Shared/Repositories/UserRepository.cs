using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;

namespace CVaS.Shared.Repositories
{
    public class UserRepository : EntityFrameworkRepository<AppUser, int>
    {
        public UserRepository(IUnitOfWorkProvider provider) : base(provider)
        {
        }
    }
}