﻿using System;
using CVaS.BL.Common;
using CVaS.DAL;
using CVaS.DAL.Model;
using CVaS.Shared.Options;
using CVaS.Shared.Helpers;
using CVaS.Web.Authentication;
using CVaS.Web.Filters;
using CVaS.Web.Installers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using Swashbuckle.Swagger.Model;
using Newtonsoft.Json;
using Swashbuckle.SwaggerGen.Application;
using EasyNetQ;
using Microsoft.Extensions.PlatformAbstractions;
using EasyNetQ.ConnectionString;
using System.Linq;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using StackExchange.Profiling.Mvc;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;
using Microsoft.Extensions.Caching.Memory;

namespace CVaS.Web
{
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ModeOptions _modeOptions = new ModeOptions();

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            _hostingEnvironment = env;
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public System.IServiceProvider ConfigureServices(IServiceCollection services)
        {
            ConfigureOptions(services);
            ConfigureIdentity(services);

            if (_modeOptions.IsLocal)
            {
                services.AddJobsService(Configuration);
            }
            else
            {
                ConfigureBroker(services);
            }

            services.AddDatabaseServices(Configuration);
            services.AddStorageServices(Configuration);

            // Add mvc framework services.
            services
                .AddMvc(options => 
                {
                    options.Filters.Add(typeof(HttpExceptionFilterAttribute));
                })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.Converters.Add(new StringEnumConverter(true));
                })
                .AddXmlDataContractSerializerFormatters();
            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddMemoryCache(options => options.CompactOnMemoryPressure = true);

            // Inject an implementation of ISwaggerProvider with defaulted settings applied
            services.AddSwaggerGen(ConfigureSwagger);
            services.AddMiniProfiler();

            services.AddTransient<AppContextSeed>();
            services.AddSingleton(Configuration);

            var physicalProvider = _hostingEnvironment.ContentRootFileProvider;
            // It's null when using ef migrations tools so we need to check first to not to throw exc
            if (physicalProvider != null) services.AddSingleton(physicalProvider);

            return new Container()
                .WithDependencyInjectionAdapter(services,
                    throwIfUnresolved: type => type.Name.EndsWith("Controller"))
                .ConfigureServiceProvider<CompositionRoot>();
        }

        private static void ConfigureIdentity(IServiceCollection services)
        {
            // Set ASP.NET Identity and cookie authentication
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
                    options.Cookies.ApplicationCookie.AutomaticChallenge = false;
                })
                .AddEntityFrameworkStores<AppDbContext, int>()
                .AddUserStore<AppUserStore>()
                .AddDefaultTokenProviders();
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
            services.AddSingleton((s) => RabbitHutch.CreateBus(Configuration.GetConnectionString("RabbitMq")));
        }

        private void ConfigureOptions(IServiceCollection services)
        {
            services.AddOptions();

            services.Configure<AlgorithmOptions>(Configuration.GetSection("Algorithm"));
            services.Configure<ModeOptions>(Configuration.GetSection("Mode"));
            services.Configure<DirectoryPathOptions>(Configuration.GetSection("DirectoryPaths"));
            services.Configure<DatabaseOptions>(Configuration.GetSection("Database"));

            Configuration.GetSection("Mode").Bind(_modeOptions);
        }

        private static void ConfigureSwagger(SwaggerGenOptions options)
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, AppContextSeed contextSeed, 
            IMemoryCache cache, DryIoc.IContainer container)
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
                AuthenticationScheme = AuthenticationScheme.ApiKey,
                HeaderScheme = "Simple"
            });
            app.UseIdentity();

            app.UseMiniProfiler(new MiniProfilerOptions
            {
                RouteBasePath = "~/profiler",
                ResultsListAuthorize = (r) => true,
                SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter(),
                Storage = new MemoryCacheStorage(cache, TimeSpan.FromMinutes(60))
            });

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

            contextSeed.SeedAsync()
                .Wait();

            if (_modeOptions.IsLocal)
            {
                ServicesExtensions.InitializeJobs(container);
            }
        }
    }
}
