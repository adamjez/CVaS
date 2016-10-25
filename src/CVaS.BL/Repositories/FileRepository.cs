using CVaS.DAL;
using CVaS.DAL.Model;

namespace CVaS.BL.Repositories
{
    public class FileRepository : AppRepositoryBase<File, int>
    {
        public FileRepository(AppDbContext context) : base(context)
        {
        }

    }
}