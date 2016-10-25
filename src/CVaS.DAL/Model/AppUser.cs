using System.Collections;
using System.Collections.Generic;
using CVaS.DAL.Common;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CVaS.DAL.Model
{
    public class AppUser : IdentityUser<int>, IEntity<int>
    {
        public ICollection<File> Files { get; private set; } = new List<File>();
    }
}