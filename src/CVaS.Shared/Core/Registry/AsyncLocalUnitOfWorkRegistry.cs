using System.Collections.Generic;
using System.Threading;

namespace CVaS.Shared.Core.Registry
{
    public class AsyncLocalUnitOfWorkRegistry : UnitOfWorkRegistryBase
    {
        private readonly AsyncLocal<Stack<IUnitOfWork>> asyncLocalStack = new AsyncLocal<Stack<IUnitOfWork>>();

        protected internal override Stack<IUnitOfWork> GetStack()
        {
            return asyncLocalStack.Value ?? (asyncLocalStack.Value = new Stack<IUnitOfWork>());
        }
    }
}