using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CVaS.BL.Installers;
using CVaS.DAL;
using CVaS.DAL.Model;
using CVaS.Web.Authentication;
using CVaS.Web.Filters;
using CVaS.Web.Installers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySQL.Data.EntityFrameworkCore.Extensions;

namespace CVaS.Web
{
    public class Startup
    {
        private readonly IHostingEnvironment hostingEnvironment;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            hostingEnvironment = env;
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            if (hostingEnvironment.IsProduction())
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseMySQL(Configuration.GetConnectionString("DefaultConnection")));
            }
            else
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            }

            services.AddIdentity<AppUser, AppRole>(options =>
                {
                    // Password settings
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;

                    // User settings
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<AppDbContext, int>()
                .AddDefaultTokenProviders();

            // Add framework services.
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(HttpExceptionFilterAttribute));
            });

            // Register Autofac
            var containerBuilder = new ContainerBuilder();

            // Add application services.
            var physicalProvider = hostingEnvironment.ContentRootFileProvider;
            containerBuilder.RegisterInstance(physicalProvider);
            containerBuilder.RegisterInstance(Configuration);
            containerBuilder.RegisterType<DbInitializer>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Register Modules
            containerBuilder.RegisterModule<BusinessLayerModule>();
            containerBuilder.RegisterModule<WebApiModule>();

            // Populate asp.net core services to autofac
            containerBuilder.Populate(services);
            var container = containerBuilder.Build();

            return new AutofacServiceProvider(container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, DbInitializer initializer)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseIdentity();
            app.UseCustomAuthentication();

            initializer.Initialize();

            app.UseMvc();
        }
    }
}
