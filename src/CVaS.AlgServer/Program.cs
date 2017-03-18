using CVaS.AlgServer.Options;
using CVaS.AlgServer.Services.Server;

namespace CVaS.AlgServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var env = EnviromentOptions.GetEnviromentOptions();
            var startup = new Startup(env);

            using (var provider = startup.ConfigureServices()) 
            {
                startup.Configure(provider);

                var server = (BrokerServer)provider.GetInstance(typeof(BrokerServer));
                server.StartAndWait();
            }
        }
    }
}
