using System;
using System.Threading.Tasks;
using CVaS.DAL;
using CVaS.Shared.Core.Provider;
using Microsoft.EntityFrameworkCore;

namespace CVaS.Shared.Core
{
    /// <summary>
    /// An implementation of unit of work in Entity ramework.
    /// </summary>
    public class EntityFrameworkUnitOfWork : UnitOfWorkBase
    {
        private readonly bool canCommit;
        private readonly bool ownContext;

        /// <summary>
        /// Gets the <see cref="Microsoft.EntityFrameworkCore.DbContext"/>.
        /// </summary>
        public override AppDbContext Context { get; }

        /// <summary>
        /// If TransactionalMode is enabled, child unit of works are not allowed to commit changes.
        /// </summary>
        public bool TransactionalModeEnabled { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFrameworkUnitOfWork"/> class.
        /// </summary>
        public EntityFrameworkUnitOfWork(IUnitOfWorkProvider provider, Func<AppDbContext> dbContextFactory, DbContextOptions options)
        {
            if (options.HasFlag(DbContextOptions.DisableTransactionMode))
            {
                TransactionalModeEnabled = false;
            }

            if (options.HasFlag(DbContextOptions.ReuseParentContext))
            {
                var parentUow = provider.GetCurrent() as EntityFrameworkUnitOfWork;
                if (parentUow != null)
                {
                    this.Context = parentUow.Context;
                    if (!parentUow.TransactionalModeEnabled)
                    {
                        canCommit = true;
                    }

                    return;
                }
            }

            this.Context = dbContextFactory();
            ownContext = true;
            canCommit = true;
        }



        /// <summary>
        /// Commits this instance when we have to.s
        /// </summary>
        public override async Task CommitAsync()
        {
            if (canCommit)
            {
                await base.CommitAsync();
            }
        }

        /// <summary>
        /// Commits the changes.
        /// </summary>
        protected override async Task CommitCoreAsync()
        {
            await Context.SaveChangesAsync();
        }

        /// <summary>
        /// Disposes the context.
        /// </summary>
        protected override void DisposeCore()
        {
            if (ownContext)
            {
                Context.Dispose();
            }
        }

        /// <summary>
        /// Tries to get the <see cref="Microsoft.EntityFrameworkCore.DbContext"/> in the current scope.
        /// </summary>
        public static DbContext TryGetDbContext(IUnitOfWorkProvider provider)
        {
            var uow = provider.GetCurrent() as EntityFrameworkUnitOfWork;
            if (uow == null)
            {
                throw new InvalidOperationException("The EntityFrameworkRepository must be used in a unit of work of type EntityFrameworkUnitOfWork!");
            }
            return uow.Context;
        }
    }
}