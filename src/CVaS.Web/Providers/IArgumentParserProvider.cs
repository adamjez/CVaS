using System.Collections.Generic;

namespace CVaS.Web.Providers
{
    public interface IArgumentParserProvider
    {
        bool CanParse(object arguments);

        List<object> Parse(object arguments);
    }
}