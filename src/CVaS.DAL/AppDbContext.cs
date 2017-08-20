using CVaS.DAL.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CVaS.DAL
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, int>
    {
        public DbSet<Algorithm> Algorithms { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Run> Runs { get; set; }
        public DbSet<Rule> Rules { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<File>()
                .HasAlternateKey(c => c.LocationId);

            modelBuilder.Entity<Algorithm>()
                .HasAlternateKey(c => c.CodeName);

            modelBuilder.Entity<Algorithm>()
                .Property(c => c.CodeName)
                .IsRequired()
                .HasMaxLength(64)
                .IsUnicode(false);
        }
    }
}
