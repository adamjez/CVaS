using System.Collections.Generic;
using System.Linq;
using CVaS.DAL;
using CVaS.DAL.Common;

namespace CVaS.BL.Repositories
{
    public abstract class AppRepositoryBase<TEntity, TKey> where TEntity : class, IEntity<TKey>, new()
    {
        protected AppDbContext Context { get; set; }

        protected AppRepositoryBase(AppDbContext context)
        {
            Context = context;
        }

        public TEntity GetById(TKey id)
        {
            return GetByIds(new TKey[] { id }).FirstOrDefault();

        }

        public virtual IList<TEntity> GetByIds(IEnumerable<TKey> ids)
        {
            IQueryable<TEntity> source = this.Context.Set<TEntity>();

            return source.Where(i => ids.Contains(i.Id)).ToList();
        }


        public virtual void Insert(TEntity entity)
        {
            Context.Set<TEntity>().Add(entity);
            Context.SaveChanges();
        }
    }
}