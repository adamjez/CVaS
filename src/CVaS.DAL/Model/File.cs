using System;
using System.ComponentModel.DataAnnotations;
using CVaS.DAL.Common;

namespace CVaS.DAL.Model
{
    public class File : IEntity<int>
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string LocationId { get; set; }
         
        public FileType Type { get; set; }

        public int UserId { get; set; }
        public virtual AppUser User { get; set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        [MaxLength(16)]
        public byte[] Hash { get; set; }

        public long FileSize { get; set; }

        public string ContentType { get; set; }

        public string Extension { get; set; }
    }
}