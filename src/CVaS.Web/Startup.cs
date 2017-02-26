using System;
using CVaS.BL.Common;
using CVaS.BL.Installers;
using CVaS.DAL;
using CVaS.DAL.Model;
using CVaS.Shared.Installers;
using CVaS.Shared.Options;
using CVaS.Shared.Services.Process;
using CVaS.Web.Authentication;
using CVaS.Web.Filters;
using CVaS.Web.Installers;
using CVaS.Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySQL.Data.EntityFrameworkCore.Extensions;
using Newtonsoft.Json.Converters;
using Swashbuckle.Swagger.Model;
using LightInject.Microsoft.DependencyInjection;
using Newtonsoft.Json;
using Swashbuckle.SwaggerGen.Application;
using AlgorithmOptions = CVaS.Shared.Services.Process.AlgorithmOptions;

namespace CVaS.Web
{
    public class Startup
    {
        private readonly IHostingEnvironment hostingEnvironment;
        private DatabaseConfiguration databaseConfiguration;
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
            services.AddOptions();
            services.Configure<BrokerOptions>(Configuration.GetSection("Broker"));
            services.Configure<AlgorithmOptions>(Configuration.GetSection("Algorithm"));

            databaseConfiguration = new DatabaseConfiguration();
            Configuration.GetSection("Database").Bind(databaseConfiguration);
            // We choose what database provider we will use
            // In configuration have to be "MySQL" or "MSSQL" 
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            if (databaseConfiguration.Provider == "MySQL")
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseMySQL(connectionString));
            }
            else
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(connectionString));
            }

            // Set ASP.NET Identity and cooie authentication
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

                    options.Cookies.ApplicationCookie.ExpireTimeSpan = TimeSpan.FromDays(150);
                    options.Cookies.ApplicationCookie.LoginPath = "/Account/Login/";
                    options.Cookies.ApplicationCookie.LogoutPath = "/Account/LogOff";
                    options.Cookies.ApplicationCookie.AuthenticationScheme = AuthenticationScheme.WebCookie;
                    options.Cookies.ApplicationCookie.AutomaticAuthenticate = true;
                    options.Cookies.ApplicationCookie.AutomaticChallenge = true;
                })
                .AddEntityFrameworkStores<AppDbContext, int>()
                .AddDefaultTokenProviders();

            // Add framework services.
            services.AddMvc(options => { options.Filters.Add(typeof(HttpExceptionFilterAttribute)); })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.Converters.Add(new StringEnumConverter(true));
                })
                .AddXmlDataContractSerializerFormatters();

            // Inject an implementation of ISwaggerProvider with defaulted settings applied
            services.AddSwaggerGen(SwaggerSetup);

            var container = new LightInject.ServiceContainer();

            var physicalProvider = hostingEnvironment.ContentRootFileProvider;
            container.RegisterInstance(physicalProvider);
            container.RegisterInstance(Configuration);
            container.Register<DbInitializer>();

            container.RegisterFrom<WebApiComposition>();
            container.RegisterFrom<BusinessLayerComposition>();

            return container.CreateServiceProvider(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, DbInitializer initializer)
        {
            initializer.Init(databaseConfiguration.DefaultUsername, 
                databaseConfiguration.DefaultEmail, 
                databaseConfiguration.DefaultPassword);

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseApiAuthentication(new ApiAuthenticationOptions()
            {
                AuthenticationScheme = AuthenticationScheme.ApiKey,
                HeaderScheme = "Simple"
            });
            app.UseIdentity();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}");
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger();

            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
            app.UseSwaggerUi();
        }

        private void SwaggerSetup(SwaggerGenOptions options)
        {
            options.SingleApiVersion(new Info
            {
                Version = "v1",
                Title = "Computer Vision as Service API",
                Description = "A simple api to run computer vision algorithms.",
                TermsOfService = "None"
            });
            options.AddSecurityDefinition("Bearer", new ApiKeyScheme()
            {
                In = "header",
                Name = "Authorization",
                Description = "Api Key Authentication",
                Type = "apiKey"
            });
            options.IncludeXmlComments(ResolvePathToXmlCommentFile());
        }

        private string ResolvePathToXmlCommentFile()
        {
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;
            var directory = System.IO.Path.GetDirectoryName(location);
            var pathToDoc = System.IO.Path.Combine(directory, Configuration["Swagger:XmlCommentFile"]);
            return pathToDoc;
        }
    }
}
