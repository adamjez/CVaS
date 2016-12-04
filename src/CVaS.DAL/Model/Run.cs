using System;
using System.ComponentModel.DataAnnotations;
using CVaS.DAL.Common;

namespace CVaS.DAL.Model
{
    public class Run : IEntity<int>
    {
        public int Id { get; set; }

        [MaxLength(256)]
        public string Path { get; set; }

        public int UserId { get; set; }
        public virtual AppUser User { get; set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public int AlgorithmId { get; set; }

        public virtual Algorithm Algorithm { get; set; }

        public string StdOut { get; set; }
        
        public string StdErr { get; set; }
    }
}