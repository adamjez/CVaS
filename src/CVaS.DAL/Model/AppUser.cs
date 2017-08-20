using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CVaS.DAL.Common;
using Microsoft.AspNetCore.Identity;

namespace CVaS.DAL.Model
{
    public class AppUser : IdentityUser<int>, IEntity<int>
    {
        public ICollection<File> Files { get; private set; } = new List<File>();

        public ICollection<Run> Runs { get; private set; } = new List<Run>();

        [Required, MaxLength(32)]
        public byte[] ApiKey { get; set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    }
}
