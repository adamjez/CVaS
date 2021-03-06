﻿using System.Linq;
using CVaS.BL.Services.ApiKey;
using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using CVaS.Shared.Options;
using Microsoft.Extensions.Options;

namespace CVaS.BL.Common
{
    public class AppContextSeed
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IApiKeyGenerator _apiKeyGenerator;
        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly IOptions<DatabaseOptions> _databaseOptions;

        public AppContextSeed(UserManager<AppUser> userManager, IApiKeyGenerator apiKeyGenerator, IUnitOfWorkProvider unitOfWorkProvider,
            IOptions<DatabaseOptions> databaseOptions)
        {
            _databaseOptions = databaseOptions;
            _userManager = userManager;
            _apiKeyGenerator = apiKeyGenerator;
            _unitOfWorkProvider = unitOfWorkProvider;
        }

        public async Task SeedAsync()
        {
            using (var uow = _unitOfWorkProvider.Create())
            {
                uow.Context.Database.Migrate();

                if (!uow.Context.Roles.Any())
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

                    uow.Context.Roles.AddRange(role1, role2);

                    uow.Context.SaveChanges();
                }

                if (!uow.Context.Users.Any())
                {
                    var user = new AppUser()
                    {
                        UserName = _databaseOptions.Value.DefaultUsername,
                        Email = _databaseOptions.Value.DefaultEmail,
                        ApiKey = _apiKeyGenerator.Generate(),
                        EmailConfirmed = true
                    };

                    await _userManager.CreateAsync(user, _databaseOptions.Value.DefaultPassword);

                    await _userManager.AddToRoleAsync(user, Roles.Admin);
                    await _userManager.AddToRoleAsync(user, Roles.Developer);

                    uow.Context.SaveChanges();
                }

                if (!uow.Context.Algorithms.Any())
                {
                    var algorithm1 = new Algorithm()
                    {
                        CodeName = "hello-world",
                        FilePath = "hello_world.py",
                        Title = "Hello World ",
                        Description = "Hello world Example App - Just print to standard output Hello World!"
                    };

                    uow.Context.Algorithms.Add(algorithm1);

                    var algorithm2 = new Algorithm()
                    {
                        CodeName = "face-detection",
                        FilePath = "face_detect.py",
                        Title = "Face Detection",
                        Description = "Detect Faces number on given Image - Print out to standard output number of faces on image!"
                    };

                    uow.Context.Algorithms.Add(algorithm2);

                    var algorithm3 = new Algorithm()
                    {
                        CodeName = "to-grayscale",
                        FilePath = "grayscale.py",
                        Title = "Conversion to grayscale",
                        Description = "Converts given image to grayscale!"
                    };

                    uow.Context.Algorithms.Add(algorithm3);

                    var algorithm4 = new Algorithm()
                    {
                        CodeName = "license-plate-recognition",
                        FilePath = "run.ps1",
                        Title = "Recognize car license plate",
                        Description = "Recognize car license plate!"
                    };

                    uow.Context.Algorithms.Add(algorithm4);

                    var algorithm5 = new Algorithm()
                    {
                        CodeName = "panorama",
                        FilePath = "panorama.py",
                        Title = "Creates panorama from two images",
                        Description = "Creates panorama from two images"
                    };

                    uow.Context.Algorithms.Add(algorithm5);

                    var algorithm6 = new Algorithm()
                    {
                        CodeName = "long-running",
                        FilePath = "long_running.py",
                        Title = "Long running script for testing purpose",
                        Description = "Long running script for testing purpose"
                    };

                    uow.Context.Algorithms.Add(algorithm6);

                    var algorithm7 = new Algorithm()
                    {
                        CodeName = "cascade",
                        FilePath = "cascade.py",
                        Title = "General Haar cascade - first conf file, second image",
                        Description = "General Haar cascade - first conf file, second image"
                    };

                    uow.Context.Algorithms.Add(algorithm7);

                    var algorithm8 = new Algorithm()
                    {
                        CodeName = "video-stabilization",
                        FilePath = "run.ps1",
                        Title = "Stabilize shake input video",
                        Description = "Stabilize shake input video"
                    };

                    uow.Context.Algorithms.Add(algorithm8);

                    var algorithm9 = new Algorithm()
                    {
                        CodeName = "text-recognition",
                        FilePath = "run.ps1",
                        Title = "Text Recognition",
                        Description = "Text recognition using Tesseract OCR"
                    };

                    uow.Context.Algorithms.Add(algorithm9);

                    var algorithm10 = new Algorithm()
                    {
                        CodeName = "colorization",
                        FilePath = "run.ps1",
                        Title = "Colorization",
                        Description = "Colorful Image Colorization"
                    };

                    uow.Context.Algorithms.Add(algorithm10);

                    var algorithm11 = new Algorithm()
                    {
                        CodeName = "age-detection",
                        FilePath = "run.ps1",
                        Title = "Age Detection",
                        Description = "Age detection from image source"
                    };

                    uow.Context.Algorithms.Add(algorithm11);

                    var algorithm12 = new Algorithm()
                    {
                        CodeName = "gender-detection",
                        FilePath = "run.ps1",
                        Title = "Gender Detection",
                        Description = "Gender detection from image source"
                    };

                    uow.Context.Algorithms.Add(algorithm12);

                    var algorithm13 = new Algorithm()
                    {
                        CodeName = "similarity",
                        FilePath = "similarity.py",
                        Title = "Image Similarity",
                        Description = "Similarity of two images"
                    };

                    uow.Context.Algorithms.Add(algorithm13);

                    uow.Context.SaveChanges();

                }
            }
        }
    }
}
