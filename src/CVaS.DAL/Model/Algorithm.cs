using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CVaS.DAL.Common;

namespace CVaS.DAL.Model
{
    public class Algorithm : IEntity<int>
    {
        public int Id { get; set; }

        public string Title { get; set; }

        [Required]
        [MaxLength(64)]
        public string CodeName { get; set; }

        [Required]
        [MaxLength(256)]
        public string FilePath { get; set; }

        public string Description { get; set; }

        public virtual ICollection<Run> Runs { get; private set; } = new Collection<Run>();
    }
}
