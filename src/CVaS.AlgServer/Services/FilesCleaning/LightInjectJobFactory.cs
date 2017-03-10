using FluentScheduler;
using LightInject;

namespace CVaS.AlgServer.Services.FilesCleaning
{
    public class LightInjectJobFactory : IJobFactory
    {
        private readonly IServiceFactory _serviceFactory;
        public LightInjectJobFactory(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        public IJob GetJobInstance<T>() where T : IJob
        {
            return _serviceFactory.GetInstance<T>();
        }
    }
}