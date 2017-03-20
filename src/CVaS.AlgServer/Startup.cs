using CVaS.AlgServer.Options;
using CVaS.AlgServer.Services.BrokerReceiver;
using CVaS.AlgServer.Services.FilesCleaning;
using CVaS.AlgServer.Services.MessageProcessor;
using CVaS.AlgServer.Services.Server;
using CVaS.Shared.Installers;
using CVaS.Shared.Options;
using CVaS.Shared.Services.Launch;
using FluentScheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EasyNetQ;
using CVaS.Shared.Helpers;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;

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


        public DryIoc.IContainer ConfigureServices()
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

            services.AddSingleton(Configuration);

            var container = new Container(rules => rules
                                                    .WithImplicitRootOpenScope()
                                                    .WithoutThrowOnRegisteringDisposableTransient()
                                                    .WithDefaultIfAlreadyRegistered(IfAlreadyRegistered.Throw))
                                .WithDependencyInjectionAdapter(services);

            SharedComposition.IsWebApplication = false;
            container.ConfigureServiceProvider<SharedComposition>();

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

        public void Configure(DryIoc.IContainer container)
        {
            var loggerFactory = container.Resolve<ILoggerFactory>();

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            JobManager.JobFactory = new DryIoCJobFactory(container);
            JobManager.JobException += (info) => loggerFactory.CreateLogger(nameof(JobManager)).LogCritical("An error just happened with a scheduled job: " + info.Exception);
            JobManager.Initialize(container.Resolve<PeriodFilesCleaningRegistry>());
        }
    }
}
