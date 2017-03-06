using System.Collections.Generic;
using CVaS.Shared.Services.Argument;

namespace CVaS.BL.Services.ArgumentTranslator
{
    public interface IArgumentTranslator
    {
        List<Argument> Process(IEnumerable<object> arg);
    }
}