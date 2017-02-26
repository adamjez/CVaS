using System.Collections.Generic;
using System.Threading.Tasks;
using CVaS.BL.DTO;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Providers;
using CVaS.Shared.Repositories;

namespace CVaS.BL.Facades
{
    public class AlgoFacade : AppFacadeBase
    {
        private readonly AlgorithmRepository _algorithmRepository;
        public AlgoFacade(IUnitOfWorkProvider unitOfWorkProvider, ICurrentUserProvider currentUserProvider, 
            AlgorithmRepository algorithmRepository) 
            : base(unitOfWorkProvider, currentUserProvider)
        {
            _algorithmRepository = algorithmRepository;
        }

        public async Task<List<AlgorithmListDTO>> GetAll()
        {
            using (var uow = UnitOfWorkProvider.Create())
            {
                return await uow.Context.Algorithms.Select(alg => new AlgorithmListDTO()
                {
                    Description = alg.Description,
                    CodeName = alg.CodeName
                }).ToListAsync();
            }
        }
    }
}