using CVaS.Web.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVaS.Web.Controllers.Web
{
    [Authorize(ActiveAuthenticationSchemes = AuthenticationScheme.WebCookie)]
    public abstract class WebController : Controller
    {
        
    }
}