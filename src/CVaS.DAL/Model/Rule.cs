using System;
using System.ComponentModel.DataAnnotations;
using CVaS.DAL.Common;

namespace CVaS.DAL.Model
{
    public class Rule : IEntity<int>
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public bool IsEnabled { get; set; }

        [Required]
        public string Regex { get; set; }

    }
}