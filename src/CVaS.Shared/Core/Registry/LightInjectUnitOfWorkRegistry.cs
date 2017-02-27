using System;
using CVaS.DAL;

namespace CVaS.Shared.Core.Registry
{
    public class LightInjectUnitOfWorkRegistry : IUnitOfWorkRegistry
    {
        private readonly Lazy<IUnitOfWork> _contextLazy;

        public LightInjectUnitOfWorkRegistry(Lazy<IUnitOfWork> contextLazy)
        {
            _contextLazy = contextLazy;
        }

        public void RegisterUnitOfWork(IUnitOfWork unitOfWork)
        {
            //throw new System.NotImplementedException();
        }

        public void UnregisterUnitOfWork(IUnitOfWork unitOfWork)
        {
            //throw new System.NotImplementedException();
        }

        public IUnitOfWork GetCurrent()
        {
            return _contextLazy.Value;
        }
    }
}