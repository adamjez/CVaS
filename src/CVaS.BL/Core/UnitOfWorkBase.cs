using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CVaS.DAL;

namespace CVaS.BL.Core
{
    /// <summary>
    /// A base implementation of unit of work object.
    /// </summary>
    public abstract class UnitOfWorkBase : IUnitOfWork
    {

        private readonly List<Action> afterCommitActions = new List<Action>();
        private bool isDisposed = false;

        public event EventHandler Disposing;

        public abstract AppDbContext Context { get; }

        /// <summary>
        /// Commits the changes made inside this unit of work.
        /// </summary>
        public virtual async Task CommitAsync()
        {
            await CommitCoreAsync();

            foreach (var action in afterCommitActions)
            {
                action();
            }
            afterCommitActions.Clear();
        }

        /// <summary>
        /// Performs the real commit work.
        /// </summary>
        protected abstract Task CommitCoreAsync();

        /// <summary>
        /// Registers an action to be executed after the work is committed.
        /// </summary>
        public void RegisterAfterCommitAction(Action action)
        {
            afterCommitActions.Add(action);
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            OnDisposing();
            DisposeCore();
        }

        /// <summary>
        /// Performs the real dispose work.
        /// </summary>
        protected abstract void DisposeCore();



        /// <summary>
        /// Called when the unit of work is being disposed.
        /// </summary>
        protected virtual void OnDisposing()
        {
            var handler = Disposing;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}