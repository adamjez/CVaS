using CVaS.BL.Providers;
using CVaS.Web.Authentication;
using CVaS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVaS.Web.Controllers.Web
{
    [Authorize(ActiveAuthenticationSchemes = AuthenticationScheme.WebCookie)]
    public abstract class WebController : Controller
    {
        protected readonly ICurrentUserProvider CurrentUserProvider;

        protected WebController(ICurrentUserProvider currentUserProvider)
        {
            CurrentUserProvider = currentUserProvider;
        }

        public LayoutViewModel InitializeLayoutModel(string title, string returnUrl = null)
        {
            var layout = new LayoutViewModel
            {
                Title = title,
                ReturnUrl = returnUrl
            };

            return InitializeLayoutModel(layout);
        }

        public LayoutViewModel InitializeLayoutModel(LayoutViewModel layout = null)
        {
            if (layout == null)
            {
               layout = new LayoutViewModel();
            }

            layout.SignedIn = CurrentUserProvider.Exists;
            layout.CurrentUserName = CurrentUserProvider.UserName;

            return layout;
        }

        public ViewResult ErrorView(string title = "Error")
        {
            var viewModel = new LayoutViewModel()
            {
                Title = title
            };

            InitializeLayoutModel(viewModel);

            return View("Error", viewModel);
        }
    }
}