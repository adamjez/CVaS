using System;
using CVaS.BL.Common;
using CVaS.BL.Core.Provider;
using CVaS.BL.Core.Registry;
using CVaS.BL.Facades;
using CVaS.BL.Helpers;
using CVaS.BL.Repositories;
using CVaS.BL.Services.ApiKey;
using CVaS.BL.Services.ArgumentTranslator;
using CVaS.BL.Services.Email;
using CVaS.BL.Services.File;
using CVaS.DAL;
using LightInject;
using Microsoft.EntityFrameworkCore;

namespace CVaS.BL.Installers
{
    public class BusinessLayerComposition : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            serviceRegistry.Register<AppUserStore>();
            serviceRegistry.Register<AppUserManager>(new PerRequestLifeTime());
            serviceRegistry.Register<AppSignInManager>(new PerRequestLifeTime());

            serviceRegistry.Register<Func<AppDbContext>>(c =>
            {
                var options = c.TryGetInstance<DbContextOptions<AppDbContext>>();
                return () => new AppDbContext(options);
            });

            serviceRegistry.Register<IUnitOfWorkProvider, EntityFrameworkUnitOfWorkProvider>(new PerContainerLifetime());

            serviceRegistry.Register<UnitOfWorkRegistryBase, ThreadLocalUnitOfWorkRegistry>();
            serviceRegistry.Register<IUnitOfWorkRegistry, HttpContextUnitOfWorkRegistry>();

            serviceRegistry.Register<AlgoFacade>();
            serviceRegistry.Register<FileFacade>();
            serviceRegistry.Register<RunFacade>();

            serviceRegistry.Register<FileRepository>();
            serviceRegistry.Register<AlgorithmRepository>();
            serviceRegistry.Register<RunRepository>();
            serviceRegistry.Register<UserRepository>();

            serviceRegistry.Register<AlgorithmFileProvider>();
            serviceRegistry.Register<TemporaryFileProvider>();
            serviceRegistry.Register<FileProvider>();

            serviceRegistry.Register<IApiKeyGenerator, RndApiKeyGenerator>();
            serviceRegistry.Register<IArgumentTranslator, BaseArgumentTranslator>();
            serviceRegistry.Register<IEmailSender, MockMessageSender>();

            serviceRegistry.Register<ICurrentTimeProvider, UtcNowTimeProvider>(new PerContainerLifetime());
        }
    }
}
