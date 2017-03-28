using CVaS.BL.Providers;
using CVaS.DAL.Model;
using CVaS.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace CVaS.Web.ViewComponents
{
    public class MenuPanelViewComponent : ViewComponent
    {
        private readonly ICurrentUserProvider _currentUserProvider;
        public MenuPanelViewComponent(ICurrentUserProvider currentUserProvider)
        {
            _currentUserProvider = currentUserProvider;
        }

        public IViewComponentResult Invoke()
        {
            var login = new LoginPanelViewModel()
            {
                SignedIn = _currentUserProvider.Exists,
                CurrentUserName = _currentUserProvider.UserName,
                IsAdmin = _currentUserProvider.IsInRole(Roles.Admin)
            };
        
            return View(login);
        }
    }
}