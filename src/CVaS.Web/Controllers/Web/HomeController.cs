using CVaS.BL.Providers;
using CVaS.BL.Services.Broker;
using CVaS.Shared.Messages;
using CVaS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVaS.Web.Controllers.Web
{
    public class HomeController : WebController
    {
        private readonly IBrokeSender _brokerSender;

        public HomeController(ICurrentUserProvider currentUserProvider, IBrokeSender brokerSender) 
            : base(currentUserProvider)
        {
            _brokerSender = brokerSender;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var result = _brokerSender.Send(new CreateAlgorithmMessage() {AlgorithmId = 2400});


            var layout = new LayoutViewModel
            {
                Title = "Home" + result.StdOut
            };

            return View(InitializeLayoutModel(layout));
        }

        [AllowAnonymous]
        public IActionResult About()
        {
            var layout = new LayoutViewModel
            {
                Title = "About"
            };

            return View(InitializeLayoutModel(layout));
        }

    }
}