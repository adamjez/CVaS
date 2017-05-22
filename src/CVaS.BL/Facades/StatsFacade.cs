using System.Linq;
using System.Threading.Tasks;
using CVaS.BL.DTO;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Services.Broker;
using CVaS.Shared.Services.Time;
using Microsoft.EntityFrameworkCore;
using CVaS.BL.Providers;

namespace CVaS.BL.Facades
{
    public class StatsFacade : AppFacadeBase
    {
        private readonly ICurrentTimeProvider _currentTimeProvider;
        private readonly IBrokerStatus _brokerStatus;

        public StatsFacade(IUnitOfWorkProvider unitOfWorkProvider, ICurrentUserProvider currentUserProvider, ICurrentTimeProvider currentTimeProvider, 
            IBrokerStatus brokerStatus) 
            : base(unitOfWorkProvider, currentUserProvider)
        {
            _currentTimeProvider = currentTimeProvider;
            _brokerStatus = brokerStatus;
        }

        public async Task<StatsDTO> CreateStats()
        {
            var now = _currentTimeProvider.Now;
            using (var uow = UnitOfWorkProvider.Create())
            {
                var result = new StatsDTO
                {
                    RunCountLastHour = 
                        await uow.Context.Runs.CountAsync(r => r.CreatedAt > now.AddHours(-1)),

                    RunCountLastDay = 
                        await uow.Context.Runs.CountAsync(r => r.CreatedAt > now.AddDays(-1)),

                    UploadedFilesCountThisWeek = 
                        await uow.Context.Files.CountAsync(f => f.CreatedAt > now.AddDays(-7)),

                    UploadedFilesSizeThisWeek = 
                        await uow.Context.Files.Where(f => f.CreatedAt > now.AddDays(-7)).SumAsync(f => f.FileSize),

                    ActiveUserCountLastDay =
                        await uow.Context.Users.CountAsync(u => u.Runs.Any(r => r.CreatedAt > now.AddDays(-1))),

                    RegisterUserCountThisWeek = 
                        await uow.Context.Users.CountAsync(u => u.CreatedAt > now.AddDays(-7)),

                    BrokerStatus = _brokerStatus.GetStatus(),

                    BrokerClients = await _brokerStatus.GetConnectedAlgServersCount()
                };

                return result;
            }
        }
    }
}