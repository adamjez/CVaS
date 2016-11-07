using System.Threading.Tasks;
using CVaS.DAL;
using CVaS.DAL.Model;
using Microsoft.EntityFrameworkCore;

namespace CVaS.BL.Repositories
{
    public class AlgorithmRepository : AppRepositoryBase<Algorithm, int>
    {
        public AlgorithmRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Algorithm> GetByCodeNameWithArgs(string codeName)
        {
            return await Context.Algorithms
                .Include(a => a.Arguments)
                .FirstOrDefaultAsync(a => a.CodeName == codeName);
        }
    }
}