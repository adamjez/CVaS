using CVaS.DAL;
using CVaS.DAL.Model;

namespace CVaS.BL.Repositories
{
    public class AlgorithmRepository : AppRepositoryBase<Algorithm, int>
    {
        public AlgorithmRepository(AppDbContext context) : base(context)
        {
        }
    }
}