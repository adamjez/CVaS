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
        private readonly bool hasOwnContext;

        /// <summary>
        /// Gets the <see cref="Microsoft.EntityFrameworkCore.DbContext"/>.
        /// </summary>
        public override AppDbContext Context { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFrameworkUnitOfWork"/> class.
        /// </summary>
        public EntityFrameworkUnitOfWork(IUnitOfWorkProvider provider, Func<AppDbContext> dbContextFactory, DbContextOptions options)
        {
            if (options == DbContextOptions.ReuseParentContext)
            {
                var parentUow = provider.GetCurrent() as EntityFrameworkUnitOfWork;
                if (parentUow != null)
                {
                    this.Context = parentUow.Context;
                    return;
                }
            }

            this.Context = dbContextFactory();
            hasOwnContext = true;
        }



        /// <summary>
        /// Commits this instance when we have to.s
        /// </summary>
        public override async Task CommitAsync()
        {
            if (hasOwnContext)
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
            if (hasOwnContext)
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