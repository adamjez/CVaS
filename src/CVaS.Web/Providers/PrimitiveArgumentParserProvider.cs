using System.Collections.Generic;
using System.Reflection;

namespace CVaS.Web.Providers
{
    /// <summary>
    /// Parses primite arguments like bool, int, float
    /// and event String
    /// </summary>
    public class PrimitiveArgumentParserProvider : IArgumentParser
    {
        public bool CanParse(object argument)
        {
            return argument.GetType().GetTypeInfo().IsPrimitive || argument is System.String;
        }

        public List<object> Parse(object argument)
        {
            return new List<object>() { argument };
        }
    }
}