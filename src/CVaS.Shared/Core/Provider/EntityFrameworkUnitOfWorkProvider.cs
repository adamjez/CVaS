using System;
using CVaS.DAL;
using CVaS.Shared.Core.Registry;

namespace CVaS.Shared.Core.Provider
{
    /// <summary>
    /// An implementation of unit of work provider in Entity Framework.
    /// </summary>
    public class EntityFrameworkUnitOfWorkProvider : UnitOfWorkProviderBase
    {
        private readonly Func<AppDbContext> _dbContextFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFrameworkUnitOfWorkProvider"/> class.
        /// </summary>
        public EntityFrameworkUnitOfWorkProvider(IUnitOfWorkRegistry registry, Func<AppDbContext> dbContextFactory) : base(registry)
        {
            _dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// Creates the unit of work with specified options.
        /// </summary>
        public new IUnitOfWork Create(DbContextOptions options)
        {
            return CreateCore(options);
        }

        /// <summary>
        /// Creates the unit of work.
        /// </summary>
        protected sealed override IUnitOfWork CreateUnitOfWork(object parameter)
        {
            var options = (parameter as DbContextOptions?) ?? DbContextOptions.ReuseParentContext;
            return CreateUnitOfWork(_dbContextFactory, options);
        }

        /// <summary>
        /// Creates the unit of work.
        /// </summary>
        protected virtual EntityFrameworkUnitOfWork CreateUnitOfWork(Func<AppDbContext> dbContextFactory, DbContextOptions options)
        {
            return new EntityFrameworkUnitOfWork(this, dbContextFactory, options);
        }
    }
}