using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;

namespace CVaS.Shared.Repositories
{
    public class FileRepository : EntityFrameworkRepository<File, int>
    {
        public FileRepository(IUnitOfWorkProvider provider) : base(provider)
        {
        }
    }
}