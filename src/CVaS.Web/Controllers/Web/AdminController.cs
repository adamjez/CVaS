using CVaS.DAL.Model;
using CVaS.Shared.Providers;
using CVaS.Web.Authentication;
using CVaS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVaS.Web.Controllers.Web
{
    [Authorize(Roles = Roles.Admin, ActiveAuthenticationSchemes = AuthenticationScheme.WebCookie)]
    public class AdminController : WebController
    {
        public AdminController(ICurrentUserProvider currentUserProvider)
            : base(currentUserProvider)
        {
        }

        public IActionResult Index()
        {
            var layout = new LayoutViewModel
            {
                Title = "Admin Section"
            };

            return View(InitializeLayoutModel(layout));
        }
    }
}