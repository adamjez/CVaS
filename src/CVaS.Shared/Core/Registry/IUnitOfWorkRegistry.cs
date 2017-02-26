﻿namespace CVaS.Shared.Core.Registry
{
    /// <summary>
    /// An interface for unit of work registry which is responsible for maintaining active unit of work object in the current scope.
    /// </summary>
    public interface IUnitOfWorkRegistry
    {
        /// <summary>
        /// Registers a new unit of work.
        /// </summary>
        void RegisterUnitOfWork(IUnitOfWork unitOfWork);

        /// <summary>
        /// Unregisters a specified unit of work.
        /// </summary>
        void UnregisterUnitOfWork(IUnitOfWork unitOfWork);

        /// <summary>
        /// Gets the unit of work in the current scope.
        /// </summary>
        IUnitOfWork GetCurrent();
    }
}