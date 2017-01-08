using System.Collections.Generic;
using System.Threading.Tasks;
using CVaS.BL.Core.Provider;
using CVaS.BL.DTO;
using CVaS.BL.Providers;
using CVaS.BL.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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