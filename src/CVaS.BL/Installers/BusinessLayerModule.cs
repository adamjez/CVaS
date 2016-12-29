using System;
using Autofac;
using CVaS.BL.Core.Provider;
using CVaS.BL.Core.Registry;
using CVaS.BL.Facades;
using CVaS.BL.Repositories;
using CVaS.BL.Services.ApiKey;
using CVaS.BL.Services.ArgumentTranslator;
using CVaS.BL.Services.Email;
using CVaS.BL.Services.File;
using CVaS.DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CVaS.BL.Installers
{
    public class BusinessLayerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(ThisAssembly)
                .AsClosedTypesOf(typeof(EntityFrameworkRepository<,>));

            builder.Register<Func<AppDbContext>>((c) =>
            {
                var options = c.Resolve<DbContextOptions<AppDbContext>>();
                return () => new AppDbContext(options);
            });

            builder.RegisterType<EntityFrameworkUnitOfWorkProvider>()
                .As<IUnitOfWorkProvider>()
                .SingleInstance();

            builder.Register<IUnitOfWorkRegistry>(
                (c) =>
                    new HttpContextUnitOfWorkRegistry(c.Resolve<IHttpContextAccessor>(),
                        new ThreadLocalUnitOfWorkRegistry()));

            builder.RegisterAssemblyTypes(ThisAssembly)
                .AssignableTo<AppFacadeBase>();

            builder.RegisterType<AlgorithmFileProvider>();
            builder.RegisterType<TemporaryFileProvider>();
            builder.RegisterType<FileProvider>();

            builder.RegisterType<RndApiKeyGenerator>()
                .As<IApiKeyGenerator>();

            builder.RegisterType<BaseArgumentTranslator>()
                .As<IArgumentTranslator>();

            builder.RegisterType<AuthMessageSender>()
                .As<IEmailSender>();

        }
    }
}
