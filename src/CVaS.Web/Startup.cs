using System;
using System.Runtime.InteropServices;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySQL.Data.EntityFrameworkCore.Extensions;
using Newtonsoft.Json.Converters;
using Swashbuckle.Swagger.Model;

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
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (!isWindows)
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
                .AddJsonOptions(options => { options.SerializerSettings.Converters.Add(new StringEnumConverter(true)); })
                .AddXmlDataContractSerializerFormatters();

            // Inject an implementation of ISwaggerProvider with defaulted settings applied
            services.AddSwaggerGen(c =>
            {
                c.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "Computer Vision as Service API",
                    Description = "A simple api to run computer vision algorithms.",
                    TermsOfService = "None"
                });
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme()
                {
                    In = "header",
                    Name = "Authorization",
                    Description = "Api Key Authentication",
                    Type = "apiKey"
                });
                c.IncludeXmlComments(ResolvePathToXmlCommentFile());
            });

            // Register Autofac
            var containerBuilder = new ContainerBuilder();

            // Add application services.
            var physicalProvider = hostingEnvironment.ContentRootFileProvider;
            containerBuilder.RegisterInstance(physicalProvider);
            containerBuilder.RegisterInstance(Configuration);
            containerBuilder.RegisterType<DbInitializer>();

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
            initializer.Init();

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseApiAuthentication(new ApiAuthenticationOptions()
            {
                AuthenticationScheme = AuthenticationScheme.ApiKey
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

        private string ResolvePathToXmlCommentFile()
        {
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;
            var directory = System.IO.Path.GetDirectoryName(location);
            var pathToDoc = System.IO.Path.Combine(directory, Configuration["Swagger:XmlCommentFile"]);
            return pathToDoc;
        }
    }
}
