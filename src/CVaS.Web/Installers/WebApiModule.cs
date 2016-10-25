using Autofac;
using CVaS.BL.Providers;
using CVaS.BL.Services.Process;
using CVaS.Web.Providers;
using CVaS.Web.Services;

namespace CVaS.Web.Installers
{
    public class WebApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BaseProcessService>()
                .As<IProcessService>();
            builder.RegisterType<AlgorithmFileProvider>();
            builder.RegisterType<TempFileProvider>();
            builder.RegisterType<CurrentUserProvider>()
                .As<ICurrentUserProvider>();
        }
    }
}
