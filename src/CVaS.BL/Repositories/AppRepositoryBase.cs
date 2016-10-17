using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CVaS.DAL;
using CVaS.DAL.Common;
using Microsoft.EntityFrameworkCore;

namespace CVaS.BL.Repositories
{
    public abstract class AppRepositoryBase<TEntity, TKey> where TEntity : class, IEntity<TKey>, new()
    {
        protected AppDbContext Context { get; set; }

        protected AppRepositoryBase(AppDbContext context)
        {
            Context = context;
        }

        public async Task<TEntity> GetById(TKey id)
        {
            return (await GetByIds(new TKey[] { id })).FirstOrDefault();

        }

        public virtual async Task<IList<TEntity>> GetByIds(IEnumerable<TKey> ids)
        {
            IQueryable<TEntity> source = this.Context.Set<TEntity>();

            return await source.Where(i => ids.Contains(i.Id)).ToListAsync();
        }


        public virtual async Task Insert(TEntity entity)
        {
            Context.Set<TEntity>().Add(entity);
            await Context.SaveChangesAsync();
        }
    }
}