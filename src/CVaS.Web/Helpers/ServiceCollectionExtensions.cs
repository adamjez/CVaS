using System;
using System.Linq;
using CVaS.BL.Common;
using CVaS.DAL;
using CVaS.DAL.Model;
using CVaS.Shared.Options;
using CVaS.Web.Filters;
using EasyNetQ;
using EasyNetQ.ConnectionString;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CVaS.Web.Helpers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomizedMvc(this IServiceCollection services)
        {
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
                .AddXmlDataContractSerializerFormatters()
                .AddTypedRouting()
                .AddCookieTempDataProvider();

            services.AddRouting(options => options.LowercaseUrls = true);

            return services;
        }

        public static IServiceCollection AddCustomizedIdentity(this IServiceCollection services)
        {
            services.ConfigureApplicationCookie(o =>
            {
                o.LoginPath = "/Account/Login";
                o.LogoutPath = "/Account/LogOff";
                o.ExpireTimeSpan = TimeSpan.FromDays(150);
            });

            // Set ASP.NET Identity and cookie authentication
            services.AddIdentity<AppUser, AppRole>(o =>
            {
                // Password settings
                o.Password.RequireDigit = false;
                o.Password.RequiredLength = 8;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireLowercase = false;

                // User settings
                o.User.RequireUniqueEmail = true;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddUserStore<AppUserStore>()
                .AddDefaultTokenProviders();

            return services;
        }


        public static IServiceCollection AddMessageBroker(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("RabbitMq");
            var connectionStringParser = new ConnectionStringParser();
            var connectionConfiguration = connectionStringParser.Parse(connectionString);

            services.Configure<BrokerOptions>((option) =>
            {
                option.Hostname = connectionConfiguration.Hosts.First().Host;
                option.Username = connectionConfiguration.UserName;
                option.Password = connectionConfiguration.Password;
                option.Vhost = connectionConfiguration.VirtualHost;
            });
            services.AddSingleton((s) => RabbitHutch.CreateBus(configuration.GetConnectionString("RabbitMq")));

            return services;
        }

        public static IServiceCollection AddCustomOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<AlgorithmOptions>(configuration.GetSection("Algorithm"));
            services.Configure<ModeOptions>(configuration.GetSection("Mode"));
            services.Configure<DirectoryPathOptions>(configuration.GetSection("DirectoryPaths"));
            services.Configure<DatabaseOptions>(configuration.GetSection("Database"));

            return services;
        }
    }
}
