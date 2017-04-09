using System.Threading.Tasks;
using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;
using Microsoft.EntityFrameworkCore;

namespace CVaS.Shared.Repositories
{
    public class AlgorithmRepository : EntityFrameworkRepository<Algorithm, int>
    {
        public AlgorithmRepository(IUnitOfWorkProvider provider) : base(provider)
        {
        }

        public virtual async Task<Algorithm> GetByCodeNameWithArgs(string codeName)
        {
            return await Context.Algorithms
                .FirstOrDefaultAsync(a => a.CodeName == codeName);
        }
    }
}