using System.Threading.Tasks;
using CVaS.BL.Facades;
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
        private readonly AlgoFacade _algoFacade;

        public AdminController(ICurrentUserProvider currentUserProvider, AlgoFacade algoFacade)
            : base(currentUserProvider)
        {
            _algoFacade = algoFacade;
        }

        public async Task<IActionResult> Index()
        {
            var layout = new AdminSectionViewModel
            {
                Title = "Admin Section",
                Algorithms = await _algoFacade.GetAllWithStats()
            };

            InitializeLayoutModel(layout);

            return View(layout);
        }
    }
}