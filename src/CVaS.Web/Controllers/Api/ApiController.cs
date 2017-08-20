using CVaS.Web.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVaS.Web.Controllers.Api
{
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.Both)]
    public abstract class ApiController : Controller
    {
    }
}
