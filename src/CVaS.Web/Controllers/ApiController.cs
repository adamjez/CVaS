using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVaS.Web.Controllers
{
    [Authorize(ActiveAuthenticationSchemes = "ApiKey")]
    public abstract class ApiController : Controller
    {
    }
}
