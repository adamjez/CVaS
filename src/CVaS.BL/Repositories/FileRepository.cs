using CVaS.BL.Core.Provider;
using CVaS.DAL;
using CVaS.DAL.Model;

namespace CVaS.BL.Repositories
{
    public class FileRepository : EntityFrameworkRepository<File, int>
    {
        public FileRepository(IUnitOfWorkProvider provider) : base(provider)
        {
        }
    }
}