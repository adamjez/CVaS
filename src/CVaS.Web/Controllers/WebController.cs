using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVaS.Web.Controllers
{
    [Authorize(ActiveAuthenticationSchemes = "WebCookieScheme")]
    public abstract class WebController : Controller
    {
        
    }
}