using System.Threading.Tasks;
using CVaS.Shared.Messages;

namespace CVaS.AlgServer.Services.MessageProcessor
{
    public interface IMessageProcessor
    {
        Task<AlgorithmResultMessage> ProcessAsync(CreateAlgorithmMessage request);
    }
}