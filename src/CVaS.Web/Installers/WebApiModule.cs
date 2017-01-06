using System.Runtime.InteropServices;
using Autofac;
using CVaS.BL.Providers;
using CVaS.BL.Services.File;
using CVaS.BL.Services.Interpreter;
using CVaS.BL.Services.Process;
using CVaS.Web.Helpers;
using CVaS.Web.Providers;
using CVaS.Web.Services;

namespace CVaS.Web.Installers
{
    public class WebApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isWindows)
            {
                builder.RegisterType<ConfigInterpreterResolver>()
                    .As<IInterpreterResolver>();
                builder.Register<IProcessService>(
                    (c) => new WindowsDecoratorProcessService(
                        new BaseProcessService(c.Resolve<FileProvider>()), c.Resolve<IInterpreterResolver>(), c.Resolve<FileProvider>()));
            }
            else
            {
                builder.RegisterType<BaseProcessService>()
                    .As<IProcessService>();
            }

            builder.RegisterType<CurrentUserProvider>()
                .As<ICurrentUserProvider>();

            builder.RegisterType<JsonArgumentParserProvider>()
                .As<IArgumentParserProvider>();

            builder.RegisterType<PrimitiveArgumentParserProvider>()
                .As<IArgumentParserProvider>();
        }
    }
}
