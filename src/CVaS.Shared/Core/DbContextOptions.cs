﻿using System;

namespace CVaS.Shared.Core
{
    /// <summary>
    /// Options for datacontext behavior in <see cref="EntityFrameworkUnitOfWork"/> instance.
    /// </summary>
    [Flags]
    public enum DbContextOptions
    {
        /// <summary>
        /// If we are already in another unit of work, its DbContext will be reused and the changes will be committed after the outer unit of work commits.
        /// </summary>
        ReuseParentContext = 0,

        /// <summary>
        /// This unit of work is standalone, has its own DbContext and doesn't depend on any other unit of work instances.
        /// </summary>
        AlwaysCreateOwnContext = 1,
        
        /// <summary>
        /// This unit of work let childs unit of works commit independently.
        /// </summary>
        DisableTransactionMode = 2
    }
}