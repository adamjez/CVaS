﻿using CVaS.BL.Services.Broker;
using CVaS.Web.Providers;
using DryIoc;
using CVaS.Shared.Options;
using CVaS.BL.Installers;
using Microsoft.Extensions.Options;
using CVaS.BL.Providers;

namespace CVaS.Web.Installers
{
    public class WebApiCompositionRoot
    {
        public WebApiCompositionRoot(IRegistrator registrator, IOptions<ModeOptions> options)
        {
            BusinessLayerComposition.ModeOptions = options.Value;
            var blComposition = new BusinessLayerComposition(registrator);

            registrator.Register<RabbitMqBrokerSender>(Reuse.Singleton);
            registrator.Register<IBrokerSender, EasyNetQBrokerSender>(Reuse.InCurrentScope);

            registrator.Register<ICurrentUserProvider, CurrentUserProvider>(Reuse.InCurrentScope);

            registrator.Register<IArgumentParser, JsonArgumentParser>();
            registrator.Register<IArgumentParser, PrimitiveArgumentParser>();
        }
    }
}
