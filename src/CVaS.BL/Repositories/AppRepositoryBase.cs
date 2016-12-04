using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CVaS.BL.Core.Provider;
using CVaS.BL.Exceptions;
using CVaS.DAL;
using CVaS.DAL.Common;
using Microsoft.EntityFrameworkCore;

namespace CVaS.BL.Repositories
{
    public abstract class EntityFrameworkRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IUnitOfWorkProvider _provider;

        protected AppDbContext Context => _provider.GetCurrent().Context;

        protected EntityFrameworkRepository(IUnitOfWorkProvider provider)
        {
            _provider = provider;
        }

        public async Task<TEntity> GetById(TKey id)
        {
            return (await GetByIds(new TKey[] { id })).FirstOrDefault();
        }

        public async Task<TEntity> GetByIdSafely(TKey id)
        {
            var result = await GetById(id);

            if (result == null)
            {
                throw new NotFoundException();
            }

            return result;
        }

        public virtual async Task<IList<TEntity>> GetByIds(IEnumerable<TKey> ids)
        {
            IQueryable<TEntity> source = this.Context.Set<TEntity>();

            return await source.Where(i => ids.Contains(i.Id)).ToListAsync();
        }


        public virtual void Insert(TEntity entity)
        {
            Context.Set<TEntity>().Add(entity);
        }
    }
}