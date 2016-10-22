using CVaS.DAL.Common;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CVaS.DAL.Model
{
    public class AppUser : IdentityUser<int, AppUserClaim, AppUserRole, AppUserLogin>, IEntity<int>
    {

    }
}