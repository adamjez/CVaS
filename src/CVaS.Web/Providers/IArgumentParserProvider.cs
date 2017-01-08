using System.Collections.Generic;

namespace CVaS.Web.Providers
{

    /// <summary>
    /// Interface for parser of arguments 
    /// when starting algorithm
    /// </summary>
    public interface IArgumentParserProvider
    {
        bool CanParse(object arguments);

        List<object> Parse(object arguments);
    }
}