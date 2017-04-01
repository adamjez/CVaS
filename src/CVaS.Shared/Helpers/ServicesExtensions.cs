using CVaS.DAL;
using CVaS.Shared.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using MySQL.Data.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Microsoft.WindowsAzure.Storage;
using CVaS.Shared.Services.File.User;
using CVaS.Shared.Services.FilesCleaning;
using FluentScheduler;
using Microsoft.Extensions.Logging;
using DryIoc;

namespace CVaS.Shared.Helpers
{
    public static class ServicesExtensions
    {
        public static void AddDatabaseServices(this IServiceCollection services, IConfigurationRoot configuration)
        {
            // We choose what database provider we will use
            var msSqlConnectionString = configuration.GetConnectionString("MsSql");
            var connectionString = msSqlConnectionString ?? configuration.GetConnectionString("MySql");

            if (connectionString == null)
            {
                Console.WriteLine("ERROR: Connection string is empty");
                Console.WriteLine(
                    "Most common cause is missing environment variable with (e.g. NETCORE_ENVIRONMENT=Development)");
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (msSqlConnectionString != null)
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(connectionString));
            }
            else
            {
                services.AddDbContext<AppDbContext>(options =>
                      options.UseMySQL(connectionString));
            }
        }

        public static void AddStorageServices(this IServiceCollection services, IConfigurationRoot configuration)
        {
            var mongoDbConnectionString = configuration.GetConnectionString("MongoDb");
            var azureStorageConnectionString = configuration.GetConnectionString("AzureStorage");
            if (mongoDbConnectionString != null)
            {
                // One Client Per Application: Source http://mongodb.github.io/mongo-csharp-driver/2.2/getting_started/quick_tour/
                services.AddSingleton((sf) => new MongoClient(configuration.GetConnectionString("MongoDb")).GetDatabase("fileDb"));
                services.AddTransient<IFileStorage, MongoDbFileStorage>();
            }
            else if (azureStorageConnectionString != null)
            {
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(azureStorageConnectionString);
                services.AddSingleton(storageAccount);
                services.AddTransient<IFileStorage, AzureBlobStorage>();
            }
            else
            {
                services.AddTransient<IFileStorage, FileSystemStorage>();
            }
        }

        public static void AddJobsService(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<FilesCleaningOptions>(configuration.GetSection("FilesCleaning"));
            services.AddTransient<PeriodFilesCleaningRegistry>();
            services.AddTransient<FilesScanningAndCleaningJob>();
        }

        public static void InitializeJobs(IContainer container)
        {
            var loggerFactory = container.Resolve< ILoggerFactory>();

            JobManager.JobFactory = new DryIoCJobFactory(container);
            JobManager.JobException += (info) => loggerFactory.CreateLogger(nameof(JobManager)).LogCritical("An error just happened with a scheduled job: " + info.Exception);
            JobManager.Initialize(container.Resolve<PeriodFilesCleaningRegistry>());
        }
    }
}
