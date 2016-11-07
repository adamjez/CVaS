using CVaS.DAL.Common;

namespace CVaS.DAL.Model
{
    public class Argument : IEntity<int>
    {
        public int Id { get; set; }

        public ArgumentType Type { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}