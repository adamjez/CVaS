using CVaS.DAL.Common;

namespace CVaS.DAL.Model
{
    public class Algorithm : IEntity<int>
    {
        public int Id { get; set; }

        public string Title { get; set; }
    }
}
