using System;
using CVaS.AlgServer.Options;
using CVaS.AlgServer.Services.BrokerReceiver;
using CVaS.AlgServer.Services.FilesCleaning;
using CVaS.AlgServer.Services.MessageProcessor;
using CVaS.AlgServer.Services.Server;
using CVaS.DAL;
using CVaS.Shared.Installers;
using CVaS.Shared.Options;
using CVaS.Shared.Services.File.Providers;
using CVaS.Shared.Services.Launch;
using FluentScheduler;
using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MySQL.Data.EntityFrameworkCore.Extensions;

namespace CVaS.AlgServer
{
    public class Startup
    {
        public Startup(IEnviromentOptions env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }


        public ServiceContainer ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddOptions();
            services.Configure<DirectoryPathOptions>(Configuration.GetSection("DirectoryPaths"));

            ConfigureAlgorithmServices(services);
            ConfigureBrokerServices(services);
            ConfigureJobsServices(services);
            ConfigureDatabaseServices(services);

            var container = new LightInject.ServiceContainer();
            container.CreateServiceProvider(services);

            container.RegisterFrom<SharedComposition>();
            container.RegisterInstance(Configuration);
            // Just hack to not to try dispose container bcs of StackOverflow Exception (container calling dispose on container ..)
            container.Register<IServiceFactory>(factory => factory);
            container.Register<IMongoDatabase>(
                (sf) => new MongoClient(Configuration.GetConnectionString("MongoDb")).GetDatabase("fileDb"),
                new PerContainerLifetime());
            container.Register<IUserFileProvider, DbUserFileProvider>();

            return container;
        }

        private void ConfigureDatabaseServices(IServiceCollection services)
        {
            var databaseConfiguration = new DatabaseOptions();
            Configuration.GetSection("Database").Bind(databaseConfiguration);
            // We choose what database provider we will use
            // In configuration have to be "MySQL" or "MSSQL" 
            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            if (connectionString == null)
            {
                Console.WriteLine("ERROR: Connection string is empty");
                Console.WriteLine(
                    "Most common cause is missing environment variable with (e.g. NETCORE_ENVIRONMENT=Development)");
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
        }

        private void ConfigureAlgorithmServices(IServiceCollection services)
        {
            services.Configure<AlgorithmOptions>(Configuration.GetSection("Algorithm"));
            services.AddTransient<ILaunchService, LocalLaunchService>();
        }

        private void ConfigureBrokerServices(IServiceCollection services)
        {
            services.Configure<BrokerOptions>(Configuration.GetSection("Broker"));
            services.AddTransient<IBrokerReceiver, EasyNetQReceiver>();
            services.AddTransient<IMessageProcessor, RunMessageProcessor>();
            services.AddTransient<BrokerServer>();
        }

        private void ConfigureJobsServices(IServiceCollection services)
        {
            services.Configure<FilesCleaningOptions>(Configuration.GetSection("FilesCleaning"));
            services.AddTransient<PeriodFilesCleaningRegistry>();
            services.AddTransient<FilesScanningAndCleaningJob>();
        }

        public void Configure(ServiceContainer container)
        {
            var loggerFactory = container.GetInstance<ILoggerFactory>();

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            JobManager.JobFactory = new LightInjectJobFactory(container);
            JobManager.JobException += (info) => loggerFactory.CreateLogger(nameof(JobManager)).LogCritical("An error just happened with a scheduled job: " + info.Exception);
            JobManager.Initialize(container.GetInstance<PeriodFilesCleaningRegistry>());
        }
    }
}
