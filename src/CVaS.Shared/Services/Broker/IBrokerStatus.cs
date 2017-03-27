using System;
using System.Threading.Tasks;

namespace CVaS.Shared.Services.Broker
{
    public interface IBrokerStatus
    {
        Task<int?> GetConnectedAlgServersCount();

        string GetStatus();
    }
}