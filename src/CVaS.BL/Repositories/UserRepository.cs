using CVaS.BL.Core.Provider;
using CVaS.DAL.Model;

namespace CVaS.BL.Repositories
{
    public class UserRepository : EntityFrameworkRepository<AppUser, int>
    {
        public UserRepository(IUnitOfWorkProvider provider) : base(provider)
        {
        }
    }
}