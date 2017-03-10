using System;
using System.Linq;
using System.Threading.Tasks;
using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;
using Microsoft.EntityFrameworkCore;

namespace CVaS.Shared.Repositories
{
    public class FileRepository : EntityFrameworkRepository<File, int>
    {
        public FileRepository(IUnitOfWorkProvider provider) : base(provider)
        {
        }

        public async Task<File> GetByHashAsync(Byte[] hash, int userId)
        {
            return await Context.Files
                .Where(f => f.Hash == hash && f.UserId == userId)
                .FirstOrDefaultAsync();
        }
    }
}