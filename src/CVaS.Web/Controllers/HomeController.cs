using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVaS.Web.Controllers
{
    public class HomeController : WebController
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

    }
}