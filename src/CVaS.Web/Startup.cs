using System;
using CVaS.BL.Common;
using CVaS.BL.Installers;
using CVaS.DAL;
using CVaS.DAL.Model;
using CVaS.Shared.Options;
using CVaS.Shared.Services.File.Providers;
using CVaS.Web.Authentication;
using CVaS.Web.Filters;
using CVaS.Web.Installers;
using LightInject;
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
using MongoDB.Driver;
using Newtonsoft.Json;
using Swashbuckle.SwaggerGen.Application;
using EasyNetQ;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using EasyNetQ.ConnectionString;
using System.Linq;

namespace CVaS.Web
{
    public class Startup
    {
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly DatabaseOptions _databaseOptions = new DatabaseOptions();
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
        public System.IServiceProvider ConfigureServices(IServiceCollection services)
        {
            ConfigureOptions(services);
            ConfigureDatabase(services);
            ConfigureIdentity(services);
            ConfigureBroker(services);
            ConfigureFileStorage(services);

            // Add framework services.
            services.AddMvc(options => { options.Filters.Add(typeof(HttpExceptionFilterAttribute)); })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.Converters.Add(new StringEnumConverter(true));
                })
                .AddXmlDataContractSerializerFormatters();

            services.AddMemoryCache(options => options.CompactOnMemoryPressure = true);

            // Inject an implementation of ISwaggerProvider with defaulted settings applied
            services.AddSwaggerGen(SwaggerSetup);

            var container = new ServiceContainer();

            var physicalProvider = hostingEnvironment.ContentRootFileProvider;
            // It's null when using ef migrations tools so we need to check first to not to throw exc
            if (physicalProvider != null) container.RegisterInstance(physicalProvider);

            container.RegisterInstance(Configuration);
            container.Register<AppContextSeed>();

            container.RegisterFrom<WebApiComposition>();
            container.RegisterFrom<BusinessLayerComposition>();

            return container.CreateServiceProvider(services);
        }

        private static void ConfigureIdentity(IServiceCollection services)
        {
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
                    options.Cookies.ApplicationCookie.AuthenticationScheme = Authentication.AuthenticationScheme.WebCookie;
                    options.Cookies.ApplicationCookie.AutomaticAuthenticate = true;
                    options.Cookies.ApplicationCookie.AutomaticChallenge = true;
                })
                .AddEntityFrameworkStores<AppDbContext, int>()
                .AddUserStore<AppUserStore>()
                .AddDefaultTokenProviders();
        }

        private void ConfigureDatabase(IServiceCollection services)
        {
            // We choose what database provider we will use
            // In configuration have to be "MySQL" or "MSSQL" 
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            if (_databaseOptions.Provider == "MySQL")
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseMySQL(connectionString));
            }
            else
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(connectionString));
            }
        }

        private void ConfigureBroker(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("RabbitMq");
            var connectionStringParser = new ConnectionStringParser();

            var connectionConfiguration = connectionStringParser.Parse(connectionString);
            services.Configure<BrokerOptions>((option) => { 
                option.Hostname = connectionConfiguration.Hosts.First().Host;
                option.Username = connectionConfiguration.UserName;
                option.Password = connectionConfiguration.Password;
                option.Vhost = connectionConfiguration.VirtualHost;
            });
            services.AddSingleton(RabbitHutch.CreateBus(connectionString));
        }

        private void ConfigureFileStorage(IServiceCollection container)
        {
            var mongoDbConnectionString = Configuration.GetConnectionString("MongoDb");
            var azureStorageConnectionString = Configuration.GetConnectionString("AzureStorage");
            if (mongoDbConnectionString != null)
            {
                // One Client Per Application: Source http://mongodb.github.io/mongo-csharp-driver/2.2/getting_started/quick_tour/
                container.AddSingleton((sf) => new MongoClient(Configuration.GetConnectionString("MongoDb")).GetDatabase("fileDb"));
                container.AddTransient<IUserFileProvider, DbUserFileProvider>();
            }
            else if (azureStorageConnectionString != null)
            {
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(azureStorageConnectionString);

                container.AddSingleton(storageAccount);
                container.AddTransient<IUserFileProvider, AzureStorageProvider>();
            }
            else
            {
                container.AddTransient<IUserFileProvider, UserSystemFileProvider>();
            }
        }

        private void ConfigureOptions(IServiceCollection services)
        {
            services.AddOptions();

            services.Configure<AlgorithmOptions>(Configuration.GetSection("Algorithm"));
            services.Configure<ModeOptions>(Configuration.GetSection("Mode"));
            services.Configure<DirectoryPathOptions>(Configuration.GetSection("DirectoryPaths"));

            Configuration.GetSection("Mode").Bind(BusinessLayerComposition.ModeOptions);
            Configuration.GetSection("Database").Bind(_databaseOptions);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, AppContextSeed contextSeed)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseApiAuthentication(new ApiAuthenticationOptions()
            {
                AuthenticationScheme = Authentication.AuthenticationScheme.ApiKey,
                HeaderScheme = "Simple"
            });
            app.UseIdentity();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger();

            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
            app.UseSwaggerUi();

            contextSeed.SeedAsync(_databaseOptions.DefaultUsername,
                _databaseOptions.DefaultEmail,
                _databaseOptions.DefaultPassword)
                .Wait();
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

            var basePath = PlatformServices.Default.Application.ApplicationBasePath;
            var pathToDoc = System.IO.Path.Combine(basePath, "CVaS.Web.xml");
            options.IncludeXmlComments(pathToDoc);
        }
    }
}
