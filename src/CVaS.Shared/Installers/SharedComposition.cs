using System;
using System.Runtime.InteropServices;
using CVaS.DAL;
using CVaS.Shared.Core;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Core.Registry;
using CVaS.Shared.Repositories;
using CVaS.Shared.Services.Broker;
using CVaS.Shared.Services.File;
using CVaS.Shared.Services.Interpreter;
using CVaS.Shared.Services.Process;
using CVaS.Shared.Services.Time;
using EasyNetQ;
using LightInject;
using Microsoft.EntityFrameworkCore;

namespace CVaS.Shared.Installers
{
    public class SharedComposition : ICompositionRoot
    {
        public static bool IsWebApplication { get; set; }

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


            serviceRegistry.Register<BrokerFactory>(new PerContainerLifetime());
            serviceRegistry.Register<IBus>(s => s.GetInstance<BrokerFactory>().Bus, new PerContainerLifetime());
            serviceRegistry.Register<BrokerStatus>(new PerRequestLifeTime());
            

            serviceRegistry.Register<Func<AppDbContext>>(c =>
            {
                var options = c.TryGetInstance<DbContextOptions<AppDbContext>>();
                return () => new AppDbContext(options);
            });

            serviceRegistry.Register<IUnitOfWorkProvider, EntityFrameworkUnitOfWorkProvider>(new PerContainerLifetime());

            if (IsWebApplication)
            {
                serviceRegistry.Register<UnitOfWorkRegistryBase, ThreadLocalUnitOfWorkRegistry>();
                serviceRegistry.Register<IUnitOfWorkRegistry, HttpContextUnitOfWorkRegistry>();
            }
            else
            {
                serviceRegistry.Register<IUnitOfWorkRegistry, AsyncLocalUnitOfWorkRegistry>();
            }


            serviceRegistry.Register<FileRepository>();
            serviceRegistry.Register<AlgorithmRepository>();
            serviceRegistry.Register<RunRepository>();
            serviceRegistry.Register<UserRepository>();
            serviceRegistry.Register<RuleRepository>();

            serviceRegistry.Register<AlgorithmFileProvider>();
            serviceRegistry.Register<TemporaryFileProvider>();
            serviceRegistry.Register<FileProvider>();

            serviceRegistry.Register<ICurrentTimeProvider, UtcNowTimeProvider>(new PerContainerLifetime());

        }
    }
}
