using Autofac;
using CVaS.BL.Providers;
using CVaS.BL.Services.Process;
using CVaS.Web.Providers;
using CVaS.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace CVaS.Web.Installers
{
    public class WebApiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FileProvider>();
            builder.RegisterType<BaseProcessService>()
                .As<IProcessService>();
            builder.RegisterType<AlgorithmFileProvider>();
            builder.RegisterType<TemporaryFileProvider>();
            builder.RegisterType<CurrentUserProvider>()
                .As<ICurrentUserProvider>();

            builder.RegisterType<HttpContextAccessor>()
                .As<IHttpContextAccessor>()
                .SingleInstance();

            builder.RegisterType<ActionContextAccessor>()
                .As<IActionContextAccessor>()
                .SingleInstance();
            
            builder.RegisterType<UrlHelperFactory>()
                .As<IUrlHelperFactory>();
        }
    }
}
