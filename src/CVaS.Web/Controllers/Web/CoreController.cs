using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CVaS.BL.Facades;
using CVaS.BL.Helpers;
using CVaS.Shared.Providers;
using CVaS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace CVaS.Web.Controllers.Web
{
    public class CoreController : WebController
    {
        private readonly AlgoFacade _algoFacade;
        private readonly IRazorViewEngine _viewEngine;

        public CoreController(ICurrentUserProvider currentUserProvider, AlgoFacade algoFacade, IRazorViewEngine viewEngine)
            : base(currentUserProvider)
        {
            _algoFacade = algoFacade;
            _viewEngine = viewEngine;
        }

        public IActionResult Upload()
        {
            var layout = new LayoutViewModel
            {
                Title = "Upload Files"
            };

            return View(InitializeLayoutModel(layout));
        }

        public async Task<IActionResult> Algorithms(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var algorithm = await _algoFacade.Get(id);

            var viewName = Regex.Replace(algorithm.CodeName.Replace('-', ' ').ToTitleCase(), @"\s+", "");

            var layout = new LayoutViewModel
            {
                Title = algorithm.Title
            };

            if (!ViewExists(viewName))
            {
                return NotFound();
            }

            return View(viewName, InitializeLayoutModel(layout));
        }

        private bool ViewExists(string name)
        {
            ViewEngineResult result = _viewEngine.FindView(ControllerContext, name, false);
            return (result.View != null);
        }

    }
}