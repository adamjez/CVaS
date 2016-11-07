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
                var algorithm1 = new Algorithm()
                {
                    CodeName = "hello-world",
                    FilePath = "hello-world.py",
                    Title = "Hello World ",
                    Description = "Hello world Example App - Just print to standard output Hello World!"
                };

                _context.Algorithms.Add(algorithm1);

                var algorithm2 = new Algorithm()
                {
                    CodeName = "face-detection",
                    FilePath = "facedetect.py",
                    Title = "Face Detection",
                    Description = "Detect Faces number on given Image - Print out to standard output number of faces on image!"
                };

                algorithm2.Arguments.Add(new Argument()
                {
                    Type = ArgumentType.File,
                    Description = "Image which where will be detected faces",
                    Name = "Image"
                });

                _context.Algorithms.Add(algorithm2);

                var algorithm3 = new Algorithm()
                {
                    CodeName = "to-grayscale",
                    FilePath = "grayscale.py",
                    Title = "Conversion to grayscale",
                    Description = "Converts given image to grayscale!"
                };

                algorithm3.Arguments.Add(new Argument()
                {
                    Type = ArgumentType.File,
                    Description = "Image which where will be converted to grayscale",
                    Name = "Image"
                });

                _context.Algorithms.Add(algorithm3);

                _context.SaveChanges();
            }

            if (!_context.Roles.Any())
            {
                var role = new AppRole()
                {
                    Name = Roles.Admin,
                    NormalizedName = Roles.Admin
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
