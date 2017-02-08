using System.Runtime.InteropServices;
using CVaS.AlgServer.Options;
using CVaS.Shared.Services.Broker;
using CVaS.Shared.Services.Interpreter;
using CVaS.Shared.Services.Process;
using EasyNetQ;
using LightInject;
using Microsoft.Extensions.Options;

namespace CVaS.Shared.Installers
{
    public class BasicComposition : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isWindows)
            {
                serviceRegistry.Register<IInterpreterResolver, ConfigInterpreterResolver>();

                serviceRegistry.Register<IProcessService, BaseProcessService>();
                serviceRegistry.Decorate<IProcessService, WindowsDecoratorProcessService>();
            }
            else
            {
                serviceRegistry.Register<IProcessService, BaseProcessService>();
            }


            serviceRegistry.Register<BrokerFactory>(new PerContainerLifetime());
            serviceRegistry.Register<IBus>(s => s.GetInstance<BrokerFactory>().Bus, new PerContainerLifetime());
        }
    }
}
