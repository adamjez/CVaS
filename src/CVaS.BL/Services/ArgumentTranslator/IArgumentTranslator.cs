using System.Collections.Generic;
using System.Threading.Tasks;

namespace CVaS.BL.Services.ArgumentTranslator
{
    public interface IArgumentTranslator
    {
        Task<List<string>> ProcessAsync(IEnumerable<object> arg);
    }
}