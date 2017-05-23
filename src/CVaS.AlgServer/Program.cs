using CVaS.AlgServer.Options;
using CVaS.AlgServer.Server;
using DryIoc;

namespace CVaS.AlgServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var env = EnviromentOptions.GetEnviromentOptions();
            var startup = new Startup(env);

            using (var container = startup.ConfigureServices())
            {
                startup.Configure(container);

                var server = container.Resolve<BrokerServer>();
                server.StartAndWait();
            }
        }
    }
}
