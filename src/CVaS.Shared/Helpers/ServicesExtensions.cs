using CVaS.DAL;
using CVaS.Shared.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using MySQL.Data.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using CVaS.Shared.Services.File.Providers;
using Microsoft.WindowsAzure.Storage;

namespace CVaS.Shared.Helpers
{
    public static class ServicesExtensions
    {
        public static void AddDatabaseServices(this IServiceCollection services, IConfigurationRoot configuration)
        {
            var databaseConfiguration = new DatabaseOptions();
            configuration.GetSection("Database").Bind(databaseConfiguration);
            // We choose what database provider we will use
            // In configuration have to be "MySQL" or "MSSQL" 
            var connectionString = configuration.GetConnectionString("DefaultConnection");

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

        public static void AddStorageServices(this IServiceCollection container, IConfigurationRoot configuration)
        {
            var mongoDbConnectionString = configuration.GetConnectionString("MongoDb");
            var azureStorageConnectionString = configuration.GetConnectionString("AzureStorage");
            if (mongoDbConnectionString != null)
            {
                // One Client Per Application: Source http://mongodb.github.io/mongo-csharp-driver/2.2/getting_started/quick_tour/
                container.AddSingleton((sf) => new MongoClient(configuration.GetConnectionString("MongoDb")).GetDatabase("fileDb"));
                container.AddTransient<IUserFileProvider, DbUserFileProvider>();
            }
            else if (azureStorageConnectionString != null)
            {
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(azureStorageConnectionString);
                container.AddSingleton(storageAccount);
                container.AddTransient<IUserFileProvider, AzureStorageProvider>();
            }
            else
            {
                container.AddTransient<IUserFileProvider, UserSystemFileProvider>();
            }
        }
    }
}
