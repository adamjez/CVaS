using CVaS.BL.Common;
using CVaS.BL.Facades;
using CVaS.BL.Services.ApiKey;
using CVaS.BL.Services.ArgumentTranslator;
using CVaS.BL.Services.Email;
using CVaS.BL.Services.Launch;
using CVaS.Shared.Installers;
using CVaS.Shared.Options;
using CVaS.Shared.Services.Launch;
using LightInject;

namespace CVaS.BL.Installers
{
    public class BusinessLayerComposition : ICompositionRoot
    {
        public static ModeOptions ModeOptions { get; set; } = new ModeOptions();

        public void Compose(IServiceRegistry serviceRegistry)
        {
            SharedComposition.IsWebApplication = true;
            serviceRegistry.RegisterFrom<SharedComposition>();

            serviceRegistry.Register<ApiKeyManager>(new PerRequestLifeTime());

            //serviceRegistry.Register<AppUserStore>(new PerRequestLifeTime());
            //serviceRegistry.Register<AppUserManager>(new PerRequestLifeTime());
            //serviceRegistry.Register<AppSignInManager>(new PerRequestLifeTime());

            serviceRegistry.Register<AlgoFacade>();
            serviceRegistry.Register<FileFacade>();
            serviceRegistry.Register<RunFacade>();
            serviceRegistry.Register<RuleFacade>();
            serviceRegistry.Register<StatsFacade>();

            serviceRegistry.Register<IApiKeyGenerator, RndApiKeyGenerator>();
            serviceRegistry.Register<IArgumentTranslator, BasicArgumentTranslator>();
            serviceRegistry.Register<IEmailSender, MockMessageSender>();

            if (ModeOptions.IsLocal)
            {
                serviceRegistry.Register<ILaunchService, LocalLaunchService>();
            }
            else
            {
                serviceRegistry.Register<ILaunchService, RemoteLaunchService>();
            }

        }
    }
}
