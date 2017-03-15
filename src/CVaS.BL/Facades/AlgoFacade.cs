using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CVaS.BL.DTO;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using CVaS.DAL.Model;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Exceptions;
using CVaS.Shared.Providers;
using CVaS.Shared.Repositories;
using CVaS.Shared.Services.Time;

namespace CVaS.BL.Facades
{
    public class AlgoFacade : AppFacadeBase
    {
        private readonly AlgorithmRepository _algorithmRepository;
        private readonly ICurrentTimeProvider _currentTimeProvider;

        public AlgoFacade(IUnitOfWorkProvider unitOfWorkProvider, ICurrentUserProvider currentUserProvider, 
            AlgorithmRepository algorithmRepository, ICurrentTimeProvider currentTimeProvider) 
            : base(unitOfWorkProvider, currentUserProvider)
        {
            _algorithmRepository = algorithmRepository;
            _currentTimeProvider = currentTimeProvider;
        }

        public async Task<List<AlgorithmListDTO>> GetAll()
        {
            return await GetAll(entity => new AlgorithmListDTO()
            {
                Description = entity.Description,
                CodeName = entity.CodeName
            });
        }

        public async Task<List<AlgorithmStatsListDTO>> GetAllWithStats()
        {
            var now = _currentTimeProvider.Now();

            // Todo: Issue https://github.com/aspnet/EntityFramework/issues/7714
            return (await GetAll(entity => new
            {
                Description = entity.Description,
                CodeName = entity.CodeName,
                LaunchCount = entity.Runs.Count(),
                LaunchCountLastHour = entity.Runs.Count(r => r.CreatedAt > now.AddHours(-1)),
                LaunchCountLastDay = entity.Runs.Count(r => r.CreatedAt > now.AddDays(-1))
            }))
            .Select(entity => new AlgorithmStatsListDTO
            {
                Description = entity.Description,
                CodeName = entity.CodeName,
                LaunchCount = entity.LaunchCount,
                LaunchCountLastHour = entity.LaunchCountLastHour,
                LaunchCountLastDay = entity.LaunchCountLastDay
            })
            .ToList();
        }

        private async Task<List<TOut>> GetAll<TOut>(Expression<Func<Algorithm, TOut>> projection)
        {
            using (var uow = UnitOfWorkProvider.Create())
            {
                return await uow.Context.Algorithms.Select(projection).ToListAsync();
            }
        }

        public async Task<AlgorithmDTO> Get(string codeName)
        {
            using (UnitOfWorkProvider.Create())
            {
                var entity = await _algorithmRepository.GetByCodeNameWithArgs(codeName);

                if (entity == null)
                {
                    throw new NotFoundException("Given algorithm codeName doesn't exists");
                }

                return new AlgorithmDTO
                {
                    CodeName = entity.CodeName,
                    Description = entity.Description,
                    Title = entity.Title
                };
            }
        }
    }
}