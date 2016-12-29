using System.Collections.Generic;

namespace CVaS.Web.Helpers
{
    public interface IArgumentParserProvider
    {
        bool CanParse(object arguments);

        List<object> Parse(object arguments);
    }
}