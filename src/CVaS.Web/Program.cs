using System.IO;
using CVaS.BL.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CVaS.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                // .UseApplicationInsights()
                .Build();

            PrepareDatabase(host);

            host.Run();
        }

        private static void PrepareDatabase(IWebHost host)
        {
            var services = (IServiceScopeFactory) host.Services.GetService(typeof(IServiceScopeFactory));

            using (var scope = services.CreateScope())
            {
                var contextSeed = (AppContextSeed) scope.ServiceProvider.GetService(typeof(AppContextSeed));
                contextSeed.SeedAsync().Wait();
            }
        }
    }
}
