using CVaS.AlgServer.Options;
using CVaS.AlgServer.Services;
using CVaS.DAL;
using CVaS.Shared.Installers;
using CVaS.Shared.Options;
using CVaS.Shared.Services.Launch;
using CVaS.Shared.Services.Process;
using LightInject;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySQL.Data.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CVaS.AlgServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var env = EnviromentOptions.GetEnviromentOptions();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            var provider = ConfigureServices();
            Configure(provider);

            using (provider)
            {
                var server = provider.GetInstance<Server>();
                server.Start();
            }
        }

        public static ServiceContainer ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddOptions();
            // Configure using a sub-section of the appsettings.json file.
            services.Configure<BrokerOptions>(Configuration.GetSection("Broker"));
            services.Configure<AlgorithmOptions>(Configuration.GetSection("Algorithm"));

            services.AddTransient<IBrokerReceiver, EasyNetQReceiver>();
            services.AddTransient<IMessageProcessor, RunMessageProcessor>();
            services.AddTransient<Server>();
            services.AddTransient<ILaunchService, LocalLaunchService>();

            var databaseConfiguration = new DatabaseOptions();
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

            var container = new LightInject.ServiceContainer();
            container.RegisterInstance(Configuration);
            container.CreateServiceProvider(services);
            container.RegisterFrom<BasicComposition>();

            container.RegisterInstance<IServiceContainer>(container);

            return container;
        }

        public static void Configure(ServiceContainer container)
        {
            var loggerFactory = container.GetInstance<ILoggerFactory>();

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
        }

        public static IConfigurationRoot Configuration { get; private set; }
    }
}
