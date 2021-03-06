﻿using System;
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
using Microsoft.Extensions.PlatformAbstractions;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using StackExchange.Profiling.Storage;
using Microsoft.Extensions.Caching.Memory;
using CVaS.Web.Helpers;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using CVaS.Web.Swagger;

namespace CVaS.Web
{
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ModeOptions _modeOptions = new ModeOptions();

        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            loggerFactory
                .AddConsole(Configuration.GetSection("Logging"))
                .AddDebug();

            _hostingEnvironment = env;
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddCustomOptions(Configuration);
            Configuration.GetSection("Mode").Bind(_modeOptions);

            services.AddCustomizedIdentity();
            services.AddApiAuthentication(option =>
            {
                option.AuthenticationScheme = AuthenticationSchemes.ApiKey;
                option.HeaderScheme = "Simple";
            });

            services.AddDatabaseServices(Configuration);
            services.AddStorageServices(Configuration);
            services.AddCustomizedMvc();
            services.AddMemoryCache();
            // Inject an implementation of ISwaggerProvider with defaulted settings applied
            services.AddSwaggerGen(ConfigureSwagger);
            services.AddMiniProfiler();
                //.AddEntityFramework();

            if (_modeOptions.IsLocal)
            {
                services.AddJobsService(Configuration);
            }
            else
            {
                services.AddMessageBroker(Configuration);
            }

            services.AddTransient<AppContextSeed>();
            services.AddSingleton(Configuration);

            var physicalProvider = _hostingEnvironment.ContentRootFileProvider;
            // It's null when using ef migrations tools so we need to check first to not to throw exc
            if (physicalProvider != null) services.AddSingleton(physicalProvider);

            return new Container(Rules.Default
                    .WithCaptureContainerDisposeStackTrace()
                    .WithoutThrowIfDependencyHasShorterReuseLifespan()
                    .WithImplicitRootOpenScope())
                .WithDependencyInjectionAdapter(services,
                    throwIfUnresolved: type => type.Name.EndsWith("Controller"))  
                .ConfigureServiceProvider<WebApiCompositionRoot>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IMemoryCache cache, IContainer container)   
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseMiniProfiler(o =>
                {
                    o.RouteBasePath = "~/profiler";
                    o.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();
                    o.Storage = new MemoryCacheStorage(cache, TimeSpan.FromMinutes(20));
                });
            }

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger();
            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
                c.InjectStylesheet("/lib/swagger-ui-themes/themes/2.x/theme-flattop.css");
            });

            if (_modeOptions.IsLocal)
            {
                ServicesExtensions.InitializeJobs(container);
            }
        }

        private static void ConfigureSwagger(SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new Info
            {
                Version = "v1",
                Title = "Computer Vision as Service API",
                Description = "A simple api to run computer vision algorithms.",
                TermsOfService = "None",
                Contact = new Contact
                {
                    Name = "Adam Jež",
                    Email = "adamjez@outlook.cz"
                }
            });
            options.AddSecurityDefinition("ApiKey", new ApiKeyScheme()
            {
                In = "header",
                Name = "Authorization",
                Description = "Api Key Authentication",
                Type = "apiKey"
            });

            options.DocumentFilter<LowercaseDocumentFilter>();
            options.OperationFilter<AuthResponsesOperationFilter>();
            options.OperationFilter<AddFileParamsFilter>();


            var basePath = PlatformServices.Default.Application.ApplicationBasePath;
            var pathToDoc = System.IO.Path.Combine(basePath, "CVaS.Web.xml");
            options.IncludeXmlComments(pathToDoc);
        }
    }
}
