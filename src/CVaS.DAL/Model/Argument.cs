using CVaS.DAL.Common;
using CVaS.Web.Models;

namespace CVaS.DAL.Model
{
    public class Argument : IEntity<int>
    {
        public int Id { get; set; }

        public ArgumentType Type { get; set; }
    }
}