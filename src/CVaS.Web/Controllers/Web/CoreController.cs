using CVaS.BL.Providers;
using CVaS.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace CVaS.Web.Controllers.Web
{
    public class CoreController : WebController
    {
        public CoreController(ICurrentUserProvider currentUserProvider)
            : base(currentUserProvider)
        {
        }

        public IActionResult Upload()
        {
            var layout = new LayoutViewModel
            {
                Title = "Upload Files"
            };

            return View(InitializeLayoutModel(layout));
        }

        public IActionResult Algorithms()
        {
            var layout = new LayoutViewModel
            {
                Title = "Sample Algorithm: License Plate Recognition"
            };

            return View(InitializeLayoutModel(layout));
        }

    }
}