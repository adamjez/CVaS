using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVaS.Web.Controllers
{
    [Authorize]
    public abstract class ApiController : Controller
    {
    }
}
