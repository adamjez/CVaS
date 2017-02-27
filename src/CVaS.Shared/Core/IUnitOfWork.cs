using System;
using System.Threading.Tasks;
using CVaS.DAL;

namespace CVaS.Shared.Core
{
    /// <summary>
    /// An interface that represents a boundary of a business transaction.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Commits the changes made inside this unit of work.
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// Registers an action to be applied after the work is committed.
        /// </summary>
        void RegisterAfterCommitAction(Action action);

        /// <summary>
        /// Occurs when this unit of work is disposed.
        /// </summary>

        event EventHandler Disposing;

        /// <summary>
        /// Context for Application DB
        /// </summary>
        AppDbContext Context { get; }
    }

    public class TestUnitOfWork : UnitOfWorkBase
    {
        public TestUnitOfWork(AppDbContext context)
        {
            Context = context;
        }

        public override AppDbContext Context { get; }
        protected override async Task CommitCoreAsync()
        {
            await Context.SaveChangesAsync();
        }

        protected override void DisposeCore()
        {
            //Context.Dispose();
        }
    }
}