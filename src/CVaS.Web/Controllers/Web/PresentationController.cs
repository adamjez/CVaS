using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CVaS.BL.Facades;
using CVaS.BL.Helpers;
using CVaS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using CVaS.BL.Providers;

namespace CVaS.Web.Controllers.Web
{
    public class PresentationController : WebController
    {
        private readonly AlgorithmFacade _algorithmFacade;
        private readonly IRazorViewEngine _viewEngine;

        public PresentationController(ICurrentUserProvider currentUserProvider, AlgorithmFacade algoFacade, IRazorViewEngine viewEngine)
            : base(currentUserProvider)
        {
            _algorithmFacade = algoFacade;
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

            var algorithm = await _algorithmFacade.Get(id);

            var viewName = Regex.Replace(algorithm.CodeName.Replace('-', ' ').ToTitleCase(), @"\s+", "");

            if (!ViewExists(viewName))
            {
                return NotFound();
            }

            var layout = new LayoutViewModel
            {
                Title = algorithm.Title
            };

            return View(viewName, InitializeLayoutModel(layout));
        }

        private bool ViewExists(string name)
        {
            ViewEngineResult result = _viewEngine.FindView(ControllerContext, name, false);
            return (result.View != null);
        }

    }
}