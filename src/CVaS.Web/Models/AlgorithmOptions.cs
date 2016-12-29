using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CVaS.Web.Models
{
    public class AlgorithmOptions
    {
        public IEnumerable<object> Arguments { get; set; } = new List<object>();
    }
}
