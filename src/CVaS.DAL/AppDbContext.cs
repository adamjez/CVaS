using CVaS.DAL.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CVaS.DAL
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, int, AppUserClaim, 
        AppUserRole, AppUserLogin, AppRoleClaim, AppUserToken>
    {
        public DbSet<Algorithm> Algorithms { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }
    }
}
