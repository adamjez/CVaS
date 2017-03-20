using System;
using System.Runtime.InteropServices;
using CVaS.DAL;
using CVaS.Shared.Core;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Core.Registry;
using CVaS.Shared.Repositories;
using CVaS.Shared.Services.Broker;
using CVaS.Shared.Services.File;
using CVaS.Shared.Services.File.Algorithm;
using CVaS.Shared.Services.File.Temporary;
using CVaS.Shared.Services.Interpreter;
using CVaS.Shared.Services.Process;
using CVaS.Shared.Services.Time;
using Microsoft.EntityFrameworkCore;
using DryIoc;

namespace CVaS.Shared.Installers
{
    public class SharedComposition
    {
        public static bool IsWebApplication { get; set; }

        public SharedComposition(IRegistrator registrator)
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isWindows)
            {
                registrator.Register<IInterpreterResolver, ConfigInterpreterResolver>();

                registrator.Register<IProcessService, BaseProcessService>();
                registrator.Register<IProcessService, WindowsDecoratorProcessService>(setup: Setup.Decorator);
            }
            else
            {
                registrator.Register<IProcessService, BaseProcessService>();
            }

            registrator.Register<BrokerFactory>(Reuse.Singleton);
            registrator.Register<BrokerStatus>(Reuse.InCurrentScope);

            registrator.RegisterDelegate<AppDbContext>(r =>
            {
                var options = r.Resolve<DbContextOptions<AppDbContext>>();
                return new AppDbContext(options);
            });

            registrator.Register<IUnitOfWorkProvider, EntityFrameworkUnitOfWorkProvider>(Reuse.InCurrentScope);

            if (IsWebApplication)
            {
                registrator.Register<UnitOfWorkRegistryBase, AsyncLocalUnitOfWorkRegistry>();
                registrator.Register<IUnitOfWorkRegistry, HttpContextUnitOfWorkRegistry>();
            }
            else
            {
                registrator.Register<IUnitOfWorkRegistry, AsyncLocalUnitOfWorkRegistry>();
            }

            registrator.Register<FileRepository>();
            registrator.Register<AlgorithmRepository>();
            registrator.Register<RunRepository>();
            registrator.Register<UserRepository>();
            registrator.Register<RuleRepository>();

            registrator.Register<ITemporaryFileProvider, TemporaryFileProvider>();
            registrator.Register<FileSystemWrapper>();
            registrator.Register<UserLocalFileProvider>();
            registrator.Register<IAlgorithmFileProvider, AlgorithmFileProvider>();

            registrator.Register<ICurrentTimeProvider, UtcNowTimeProvider>(Reuse.Singleton);
        }

    }
}
