using Microsoft.AspNetCore.Mvc;

namespace CVaS.Web.Controllers.Web
{
    public class CoreController : WebController
    {
        public IActionResult Upload()
        {
            return View();
        }

        public IActionResult Algorithms()
        {
            return View();
        }
    }
}