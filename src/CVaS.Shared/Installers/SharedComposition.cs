﻿using System.Runtime.InteropServices;
using CVaS.DAL;
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

                registrator.Register<IProcessService, ProcessService>();
                registrator.Register<IProcessService, WindowsDecoratorProcessService>(setup: Setup.Decorator);
            }
            else
            {
                registrator.Register<IProcessService, ProcessService>();
            }

            registrator.Register<BrokerFactory>(Reuse.Singleton);

            if (IsWebApplication)
            {
                registrator.Register<AppDbContext>(Reuse.InCurrentScope, ifAlreadyRegistered: IfAlreadyRegistered.Replace,
                    setup: Setup.With(condition: (request) => request.ParentOrWrapper.FactoryType != FactoryType.Wrapper));

                // Registration for custom Unit Of Work Pattern => resolving using Func<AppDbContext>
                registrator.Register<AppDbContext>(Reuse.Transient,
                    ifAlreadyRegistered: IfAlreadyRegistered.AppendNotKeyed,
                    setup: Setup.With(condition: (request) => request.ParentOrWrapper.FactoryType == FactoryType.Wrapper));
            }
            else
            {
                registrator.Register<AppDbContext>(Reuse.Transient, ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            }


            if (IsWebApplication)
            {
                registrator.Register<IUnitOfWorkProvider, EntityFrameworkUnitOfWorkProvider>(Reuse.Transient);
                registrator.Register<UnitOfWorkRegistryBase, AsyncLocalUnitOfWorkRegistry>();
                registrator.Register<IUnitOfWorkRegistry, HttpContextUnitOfWorkRegistry>();
            }
            else
            {
                registrator.Register<IUnitOfWorkProvider, EntityFrameworkUnitOfWorkProvider>(Reuse.InCurrentScope);
                registrator.Register<IUnitOfWorkRegistry, AsyncLocalUnitOfWorkRegistry>();
            }

            registrator.Register<FileRepository, CachedFileRepository>();
            registrator.Register<AlgorithmRepository, CachedAlgorithmRepository>();
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
