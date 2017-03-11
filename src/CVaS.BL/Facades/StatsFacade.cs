using System.Linq;
using System.Threading.Tasks;
using CVaS.BL.DTO;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Providers;
using CVaS.Shared.Services.Broker;
using CVaS.Shared.Services.Time;
using EasyNetQ;
using Microsoft.EntityFrameworkCore;

namespace CVaS.BL.Facades
{
    public class StatsFacade : AppFacadeBase
    {
        private readonly ICurrentTimeProvider _currentTimeProvider;
        private readonly IBus _bus;
        private readonly BrokerStatus _brokerStatus;

        public StatsFacade(IUnitOfWorkProvider unitOfWorkProvider, ICurrentUserProvider currentUserProvider, ICurrentTimeProvider currentTimeProvider, 
            IBus bus, BrokerStatus brokerStatus) 
            : base(unitOfWorkProvider, currentUserProvider)
        {
            _currentTimeProvider = currentTimeProvider;
            _bus = bus;
            _brokerStatus = brokerStatus;
        }

        public async Task<StatsDTO> CreateStats()
        {
            var now = _currentTimeProvider.Now();
            using (var uow = UnitOfWorkProvider.Create())
            {
                var result = new StatsDTO
                {
                    RunCountLastHour = 
                        await uow.Context.Run.CountAsync(r => r.CreatedAt > now.AddHours(-1)),

                    RunCountLastDay = 
                        await uow.Context.Run.CountAsync(r => r.CreatedAt > now.AddDays(-1)),

                    UploadedFilesCountThisWeek = 
                        await uow.Context.Files.CountAsync(f => f.CreatedAt > now.AddDays(-7)),

                    ActiveUserCountLastDay =
                        await uow.Context.Users.CountAsync(u => u.Runs.Any(r => r.CreatedAt > now.AddDays(-1))),

                    RegisterUserCountThisWeek = 
                        await uow.Context.Users.CountAsync(u => u.CreatedAt > now.AddDays(-7)),

                    BrokerStatus = _bus.IsConnected ? "Connected" : "Disconnected",

                    //BrokerClients = await _brokerStatus.GetConnectedClients()
                };

                return result;
            }
        }
    }
}