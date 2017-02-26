using CVaS.BL.Common;
using CVaS.BL.Facades;
using CVaS.BL.Services.ApiKey;
using CVaS.BL.Services.ArgumentTranslator;
using CVaS.BL.Services.Email;
using CVaS.Shared.Installers;
using LightInject;

namespace CVaS.BL.Installers
{
    public class BusinessLayerComposition : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            serviceRegistry.RegisterFrom<BasicComposition>();

            serviceRegistry.Register<AppUserStore>();
            serviceRegistry.Register<AppUserManager>(new PerRequestLifeTime());
            serviceRegistry.Register<AppSignInManager>(new PerRequestLifeTime());

            serviceRegistry.Register<AlgoFacade>();
            serviceRegistry.Register<FileFacade>();
            serviceRegistry.Register<RunFacade>();

            serviceRegistry.Register<IApiKeyGenerator, RndApiKeyGenerator>();
            serviceRegistry.Register<IArgumentTranslator, BaseArgumentTranslator>();
            serviceRegistry.Register<IEmailSender, MockMessageSender>();

        }
    }
}
