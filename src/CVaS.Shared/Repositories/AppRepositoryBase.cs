using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CVaS.DAL;
using CVaS.DAL.Common;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CVaS.Shared.Repositories
{
    public abstract class EntityFrameworkRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IUnitOfWorkProvider _provider;

        protected AppDbContext Context => _provider.GetCurrent().Context;

        protected EntityFrameworkRepository(IUnitOfWorkProvider provider)
        {
            _provider = provider;
        }

        public Task<TEntity> GetById(TKey id)
        {
            return Context.Set<TEntity>().FindAsync(id);
        }

        public async Task<TEntity> GetById(TKey id, params Expression<Func<TEntity, object>>[] includes)
        {
            return (await GetByIds(new TKey[] { id }, includes)).FirstOrDefault();
        }

        public async Task<TEntity> GetByIdSafely(TKey id, params Expression<Func<TEntity, object>>[] includes)
        {
            var result = await GetById(id, includes);

            if (result == null)
            {
                throw new NotFoundException();
            }

            return result;
        }

        public virtual async Task<IList<TEntity>> GetByIds(IEnumerable<TKey> ids, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = Context.Set<TEntity>();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.Where(i => ids.Contains(i.Id)).ToListAsync();
        }


        public virtual void Insert(TEntity entity)
        {
            Context.Set<TEntity>().Add(entity);
        }

        public virtual void Update(TEntity entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        public virtual void Delete(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);

        }
    }
}