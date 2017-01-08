using CVaS.DAL.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CVaS.DAL
{
    public class AppUserStore : UserStore<AppUser, AppRole, AppDbContext, int>
    {
        public AppUserStore(AppDbContext context) : base(context)
        {
        }
    }
}