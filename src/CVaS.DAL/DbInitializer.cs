using System.Linq;
using CVaS.DAL.Model;
using Microsoft.AspNetCore.Identity;

namespace CVaS.DAL
{
    public class DbInitializer
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public DbInitializer(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public void Initialize()
        {
            _context.Database.EnsureCreated();

            if (!_context.Algorithms.Any())
            {
                var algorithm = new Algorithm()
                {
                    CodeName = "hello-world",
                    FilePath = "Project1",
                    Title = "Mega Giga Super Algo"
                };
                _context.Algorithms.Add(algorithm);

                _context.SaveChanges();
            }

            if (!_context.Roles.Any())
            {
                var role = new AppRole()
                {
                    Name = Roles.Admin
                };
                _context.Roles.Add(role);

                _context.SaveChanges();
            }

            if (!_context.Users.Any())
            {
                var user = new AppUser()
                {
                    UserName = "username",
                    Email = "spiritakcz@gmail.com"
                };

                _userManager.CreateAsync(user, "Password1!").Wait();

                _userManager.AddToRoleAsync(user, Roles.Admin).Wait();
            }
        }
    }
}
