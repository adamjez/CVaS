using CVaS.AlgServer.Options;
using CVaS.AlgServer.Services.BrokerReceiver;
using CVaS.AlgServer.Services.FilesCleaning;
using CVaS.AlgServer.Services.MessageProcessor;
using CVaS.AlgServer.Services.Server;
using CVaS.Shared.Installers;
using CVaS.Shared.Options;
using CVaS.Shared.Services.Launch;
using FluentScheduler;
using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EasyNetQ;
using CVaS.Shared.Helpers;

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


        public IServiceContainer ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddOptions();
            services.Configure<DirectoryPathOptions>(Configuration.GetSection("DirectoryPaths"));

            ConfigureAlgorithmServices(services);
            ConfigureBrokerServices(services);
            ConfigureJobsServices(services);
            services.AddDatabaseServices(Configuration);
            services.AddStorageServices(Configuration);

            var container = new ServiceContainer();

            container.RegisterFrom<SharedComposition>();
            container.RegisterInstance(Configuration);
            // Just hack to not to try dispose container bcs of StackOverflow Exception (container calling dispose on container ..)
            container.Register<IServiceFactory>(factory => factory);

            container.CreateServiceProvider(services);
            return container;
        }

        private void ConfigureAlgorithmServices(IServiceCollection services)
        {
            services.Configure<AlgorithmOptions>(Configuration.GetSection("Algorithm"));
            services.AddTransient<ILaunchService, LocalLaunchService>();
        }

        private void ConfigureBrokerServices(IServiceCollection services)
        {
            services.AddSingleton((s) => RabbitHutch.CreateBus(Configuration.GetConnectionString("RabbitMq")));
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

        public void Configure(IServiceContainer container)
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
