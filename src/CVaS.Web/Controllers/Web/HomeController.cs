using CVaS.BL.Providers;
using CVaS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVaS.Web.Controllers.Web
{
    public class HomeController : WebController
    {
        public HomeController(ICurrentUserProvider currentUserProvider) 
            : base(currentUserProvider)
        {
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var layout = new LayoutViewModel
            {
                Title = "Home"
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