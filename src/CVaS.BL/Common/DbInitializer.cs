using System.Linq;
using CVaS.BL.Services.ApiKey;
using CVaS.DAL;
using CVaS.DAL.Model;

namespace CVaS.BL.Common
{
    public class DbInitializer
    {
        private readonly AppUserManager _userManager;
        private readonly AppDbContext _context;
        private readonly IApiKeyGenerator _apiKeyGenerator;

        public DbInitializer(AppUserManager userManager, AppDbContext context, IApiKeyGenerator apiKeyGenerator)
        {
            _userManager = userManager;
            _context = context;
            _apiKeyGenerator = apiKeyGenerator;
        }

        public void Init(string username = null, string email = null, string password = null)
        {
            _context.Database.EnsureCreated();

            if (!_context.Roles.Any())
            {
                var role1 = new AppRole()
                {
                    Name = Roles.Admin,
                    NormalizedName = Roles.Admin
                };

                var role2 = new AppRole()
                {
                    Name = Roles.Developer,
                    NormalizedName = Roles.Developer
                };

                _context.Roles.AddRange(role1, role2);

                _context.SaveChanges();
            }

            if (!_context.Users.Any())
            {
                var user = new AppUser()
                {
                    UserName = username,
                    Email = email,
                    ApiKey = _apiKeyGenerator.Generate()
                };

                _userManager.CreateAsync(user, password).Wait();

                _userManager.AddToRoleAsync(user, Roles.Admin).Wait();
            }

            if (!_context.Algorithms.Any())
            {
                var algorithm1 = new Algorithm()
                {
                    CodeName = "hello-world",
                    FilePath = "hello_world.py",
                    Title = "Hello World ",
                    Description = "Hello world Example App - Just print to standard output Hello World!"
                };

                _context.Algorithms.Add(algorithm1);

                var algorithm2 = new Algorithm()
                {
                    CodeName = "face-detection",
                    FilePath = "face_detect.py",
                    Title = "Face Detection",
                    Description = "Detect Faces number on given Image - Print out to standard output number of faces on image!"
                };

                _context.Algorithms.Add(algorithm2);

                var algorithm3 = new Algorithm()
                {
                    CodeName = "to-grayscale",
                    FilePath = "grayscale.py",
                    Title = "Conversion to grayscale",
                    Description = "Converts given image to grayscale!"
                };

                _context.Algorithms.Add(algorithm3);

                var algorithm4 = new Algorithm()
                {
                    CodeName = "license-plate-recognition",
                    FilePath = "run.ps1",
                    Title = "Recognize car license plate",
                    Description = "Recognize car license plate!"
                };

                _context.Algorithms.Add(algorithm4);

                var algorithm5 = new Algorithm()
                {
                    CodeName = "panorama",
                    FilePath = "panorama.py",
                    Title = "Creates panorama from two images",
                    Description = "Creates panorama from two images"
                };

                _context.Algorithms.Add(algorithm5);

                var algorithm6 = new Algorithm()
                {
                    CodeName = "long-running",
                    FilePath = "long_running.py",
                    Title = "Long running script for testing purpose",
                    Description = "Long running script for testing purpose"
                };

                _context.Algorithms.Add(algorithm6);

                var algorithm7 = new Algorithm()
                {
                    CodeName = "cascade",
                    FilePath = "cascade.py",
                    Title = "General Haar cascade - first conf file, second image",
                    Description = "General Haar cascade - first conf file, second image"
                };

                _context.Algorithms.Add(algorithm7);

                var algorithm8 = new Algorithm()
                {
                    CodeName = "video-stabilization",
                    FilePath = "run.ps1",
                    Title = "Stabilize shake input video",
                    Description = "Stabilize shake input video"
                };

                _context.Algorithms.Add(algorithm8);

                _context.SaveChanges();
            }
        }
    }
}
