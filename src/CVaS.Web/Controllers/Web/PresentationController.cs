using Microsoft.AspNetCore.Mvc;

namespace CVaS.Web.Controllers.Web
{
    public class PresentationController : WebController
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