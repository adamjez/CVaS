using System;
using CVaS.AlgServer.Options;
using CVaS.AlgServer.Services.BrokerReceiver;
using CVaS.AlgServer.Services.MessageProcessor;
using CVaS.AlgServer.Services.Server;
using CVaS.DAL;
using CVaS.Shared.Installers;
using CVaS.Shared.Options;
using CVaS.Shared.Services.File.Providers;
using CVaS.Shared.Services.Launch;
using LightInject;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySQL.Data.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

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


            using (var provider = ConfigureServices())
            {
                Configure(provider);

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

            if (connectionString == null)
            {
                Console.WriteLine("ERROR: Connection string is empty");
                Console.WriteLine("Most common cause is missing environment variable with (e.g. NETCORE_ENVIRONMENT=Development)");
                throw new ArgumentNullException(nameof(connectionString));
            }

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
            container.CreateServiceProvider(services);

            container.RegisterFrom<SharedComposition>();
            container.RegisterInstance(Configuration);
            // Just hack to not to try dispose container bcs of StackOverflow Exception (container calling dispose on container ..)
            container.Register<IServiceFactory>(factory => factory);
            container.Register<IMongoDatabase>(
                (sf) => new MongoClient(Configuration.GetConnectionString("MongoDb")).GetDatabase("fileDb"),
                new PerContainerLifetime());
            container.Register<IFileProvider, DbFileProvider>();


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
