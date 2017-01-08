using System.Runtime.InteropServices;
using Autofac;
using CVaS.BL.Providers;
using CVaS.BL.Services.File;
using CVaS.BL.Services.Interpreter;
using CVaS.BL.Services.Process;
using CVaS.Web.Helpers;
using CVaS.Web.Providers;
using CVaS.Web.Services;
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

            serviceRegistry.Register<ICurrentUserProvider, CurrentUserProvider>();
            // LightInject: to register multiple classy for 1 interface, different service name
            // have to be giben
            serviceRegistry.Register<IArgumentParserProvider, JsonArgumentParserProvider>(
                nameof(JsonArgumentParserProvider));
            serviceRegistry.Register<IArgumentParserProvider, PrimitiveArgumentParserProvider>(
                nameof(PrimitiveArgumentParserProvider));
        }
    }
}
