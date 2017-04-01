using System;
using CVaS.BL.Common;
using CVaS.Shared.Options;
using CVaS.Shared.Helpers;
using CVaS.Web.Authentication;
using CVaS.Web.Installers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Application;
using Microsoft.Extensions.PlatformAbstractions;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using StackExchange.Profiling.Mvc;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;
using Microsoft.Extensions.Caching.Memory;
using CVaS.Web.Helpers;

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
            services.AddCustomOptions(Configuration);
            services.AddCustomizedIdentity();
            services.AddDatabaseServices(Configuration);
            services.AddStorageServices(Configuration);
            services.AddCustomizedMvc();
            services.AddMemoryCache(options => options.CompactOnMemoryPressure = true);

            if (_modeOptions.IsLocal)
            {
                services.AddJobsService(Configuration);
            }
            else
            {
                services.AddMessageBroker(Configuration);
            }

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
                .ConfigureServiceProvider<WebApiCompositionRoot>();
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

            if (env.IsDevelopment())
            {
                app.UseMiniProfiler(new MiniProfilerOptions
                {
                    RouteBasePath = "~/profiler",
                    ResultsListAuthorize = (r) => true,
                    SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter(),
                    Storage = new MemoryCacheStorage(cache, TimeSpan.FromMinutes(60))
                });
            }

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
    }
}
