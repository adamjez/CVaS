using System.Runtime.InteropServices;
using CVaS.BL.Services.Broker;
using CVaS.Shared.Providers;
using CVaS.Shared.Services.Interpreter;
using CVaS.Shared.Services.Process;
using CVaS.Web.Providers;
using LightInject;

namespace CVaS.Web.Installers
{
    public class WebApiComposition : ICompositionRoot
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

            serviceRegistry.Register<RabbitMqBrokerSender>(new PerContainerLifetime());
            serviceRegistry.Register<IBrokerSender, EasyNetQBrokerSender>(new PerRequestLifeTime());

            serviceRegistry.Register<ICurrentUserProvider, CurrentUserProvider>();

            // LightInject: to register multiple classy for 1 interface, 
            // different service name have to be giben
            serviceRegistry.Register<IArgumentParserProvider, JsonArgumentParserProvider>(
                nameof(JsonArgumentParserProvider));
            serviceRegistry.Register<IArgumentParserProvider, PrimitiveArgumentParserProvider>(
                nameof(PrimitiveArgumentParserProvider));
        }
    }
}
