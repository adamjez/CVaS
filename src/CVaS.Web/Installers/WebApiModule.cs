using Autofac;
using CVaS.BL.Providers;
using CVaS.BL.Services.File;
using CVaS.BL.Services.Process;
using CVaS.Web.Providers;
using CVaS.Web.Services;

namespace CVaS.Web.Installers
{
    public class WebApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register<IProcessService>(
                (c) =>new WindowsDecoratorProcessService(new BaseProcessService(c.Resolve<FileProvider>())));

            builder.RegisterType<CurrentUserProvider>()
                .As<ICurrentUserProvider>();
        }
    }
}
