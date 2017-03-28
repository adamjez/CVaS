using System.Threading.Tasks;

namespace CVaS.Shared.Services.Broker
{
    public class LocalBrokerStatus : IBrokerStatus
    {
        public Task<int?> GetConnectedAlgServersCount()
        {
            return Task.FromResult((int?)null);
        }

        public string GetStatus()
        {
            return "Local Mode";
        }
    }
}