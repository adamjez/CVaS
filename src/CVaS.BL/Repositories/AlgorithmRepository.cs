using System.Threading.Tasks;
using CVaS.BL.Core.Provider;
using CVaS.DAL;
using CVaS.DAL.Model;
using Microsoft.EntityFrameworkCore;

namespace CVaS.BL.Repositories
{
    public class AlgorithmRepository : EntityFrameworkRepository<Algorithm, int>
    {
        public AlgorithmRepository(IUnitOfWorkProvider provider) : base(provider)
        {
        }

        public async Task<Algorithm> GetByCodeNameWithArgs(string codeName)
        {
            return await Context.Algorithms
                .FirstOrDefaultAsync(a => a.CodeName == codeName);
        }
    }
}