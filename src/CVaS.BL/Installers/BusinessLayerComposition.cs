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
            BasicComposition.IsWebApplication = true;
            serviceRegistry.RegisterFrom<BasicComposition>();

            serviceRegistry.Register<ApiKeyManager>(new PerRequestLifeTime());

            serviceRegistry.Register<AppUserStore>(new PerRequestLifeTime());
            serviceRegistry.Register<AppUserManager>(new PerRequestLifeTime());
            serviceRegistry.Register<AppSignInManager>(new PerRequestLifeTime());

            serviceRegistry.Register<AlgoFacade>();
            serviceRegistry.Register<FileFacade>();
            serviceRegistry.Register<RunFacade>();

            serviceRegistry.Register<IApiKeyGenerator, RndApiKeyGenerator>();
            serviceRegistry.Register<IArgumentTranslator, BaseArgumentTranslator>();
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
