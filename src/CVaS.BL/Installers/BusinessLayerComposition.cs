using CVaS.BL.Common;
using CVaS.BL.Facades;
using CVaS.BL.Services.ApiKey;
using CVaS.BL.Services.ArgumentTranslator;
using CVaS.BL.Services.Email;
using CVaS.BL.Services.Launch;
using CVaS.Shared.Installers;
using CVaS.Shared.Options;
using CVaS.Shared.Services.Broker;
using CVaS.Shared.Services.Launch;
using DryIoc;

namespace CVaS.BL.Installers
{ 
    public class BusinessLayerComposition
    {
        public static ModeOptions ModeOptions { get; set; } = new ModeOptions();

        public BusinessLayerComposition(IRegistrator registrator)
        {
            SharedComposition.IsWebApplication = true;
            var sharedComposition = new SharedComposition(registrator);

            registrator.Register<ApiKeyManager>();

            registrator.Register<AccountFacade>();
            registrator.Register<AlgorithmFacade>();
            registrator.Register<FileFacade>();
            registrator.Register<RunFacade>();
            registrator.Register<RuleFacade>();
            registrator.Register<StatsFacade>();

            registrator.Register<IApiKeyGenerator, RndApiKeyGenerator>();
            registrator.Register<IArgumentTranslator, BasicArgumentTranslator>();
            registrator.Register<IEmailSender, MockMessageSender>();


            if (ModeOptions.IsLocal)
            {
                registrator.Register<ILaunchService, LocalLaunchService>();
                registrator.Register<IBrokerStatus, LocalBrokerStatus>(Reuse.InCurrentScope);
            }
            else
            {
                registrator.Register<ILaunchService, RemoteLaunchService>();
                registrator.Register<IBrokerStatus, BrokerStatus>(Reuse.InCurrentScope);
            }
        }

    }
}
