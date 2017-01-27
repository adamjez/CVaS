using System.Collections.Generic;
using System.Threading.Tasks;
using CVaS.BL.Providers;
using CVaS.Web.Models;
using CVaS.Web.Providers;
using Microsoft.AspNetCore.Mvc;

namespace CVaS.Web.ViewComponents
{
    public class LoginPanelViewComponent : ViewComponent
    {
        private readonly ICurrentUserProvider _currentUserProvider;
        public LoginPanelViewComponent(ICurrentUserProvider currentUserProvider)
        {
            _currentUserProvider = currentUserProvider;
        }

        public IViewComponentResult Invoke()
        {
            var login = new LoginPanelViewModel()
            {
                SignedIn = _currentUserProvider.Exists,
                CurrentUserName = _currentUserProvider.UserName
            };
        
            return View(login);
        }
    }
}