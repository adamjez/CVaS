using System.Threading.Tasks;

namespace CVaS.BL.Services.ArgumentTranslator
{
    public interface IArgumentTranslator
    {
        Task<string> ProcessAsync(object arg);
    }
}