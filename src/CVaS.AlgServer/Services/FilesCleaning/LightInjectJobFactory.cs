using DryIoc;
using FluentScheduler;

namespace CVaS.AlgServer.Services.FilesCleaning
{
    public class DryIoCJobFactory : IJobFactory
    {
        private readonly IContainer _container;
        public DryIoCJobFactory(IContainer container)
        {
            _container = container;
        }

        public IJob GetJobInstance<T>() where T : IJob
        {
            return _container.Resolve<T>();
        }
    }
}