using Autofac;
using CVaS.BL.Services.Process;
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
        }
    }
}
