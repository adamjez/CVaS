using Autofac;
using CVaS.BL.Repositories;

namespace CVaS.BL.Installers
{
    public class BusinessLayerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(ThisAssembly)
                .AsClosedTypesOf(typeof(AppRepositoryBase<,>));
        }
    }
}
