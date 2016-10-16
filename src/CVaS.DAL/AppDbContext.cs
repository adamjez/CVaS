using CVaS.DAL.Model;
using Microsoft.EntityFrameworkCore;

namespace CVaS.DAL
{
    public class AppDbContext : DbContext
    {
        public DbSet<Algorithm> Algorithms { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }
    }
}
