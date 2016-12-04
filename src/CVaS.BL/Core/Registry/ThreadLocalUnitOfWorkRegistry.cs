﻿using System.Collections.Generic;
using System.Threading;

namespace CVaS.BL.Core.Registry
{
    /// <summary>
    /// A unit of work storage which persists the unit of work instances in a ThreadLocal object.
    /// </summary>
    public class ThreadLocalUnitOfWorkRegistry : UnitOfWorkRegistryBase
    {

        private readonly ThreadLocal<Stack<IUnitOfWork>> stack
            = new ThreadLocal<Stack<IUnitOfWork>>(() => new Stack<IUnitOfWork>());

        /// <summary>
        /// Gets the stack of currently active unit of work objects.
        /// </summary>
        protected internal override Stack<IUnitOfWork> GetStack()
        {
            return stack.Value;
        }
    }
}