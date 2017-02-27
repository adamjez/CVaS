using System.Collections.Generic;
using System.Threading;

namespace CVaS.Shared.Core.Registry
{
    public class AsyncLocalUnitOfWorkRegistry : UnitOfWorkRegistryBase
    {
        private readonly AsyncLocal<Stack<IUnitOfWork>> asyncLocalStack = new AsyncLocal<Stack<IUnitOfWork>>();

        protected internal override Stack<IUnitOfWork> GetStack()
        {
            if (asyncLocalStack.Value == null)
            {
                asyncLocalStack.Value = new Stack<IUnitOfWork>();
            }
            return asyncLocalStack.Value;
        }
    }
}