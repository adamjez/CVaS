using System.IO;
using System.Threading.Tasks;
using CVaS.BL.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CVaS.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            await PrepareDatabase(host);

            host.Run();
        }

        private static async Task PrepareDatabase(IWebHost host)
        {
            var services = host.Services.GetService<IServiceScopeFactory>();

            using (var scope = services.CreateScope())
            {
                var contextSeed = scope.ServiceProvider.GetService<AppContextSeed>();
                await contextSeed.SeedAsync();
            }
        }
    }
}
