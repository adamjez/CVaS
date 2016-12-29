using System.Collections.Generic;
using System.Reflection;

namespace CVaS.Web.Providers
{
    public class PrimitiveArgumentParserProvider : IArgumentParserProvider
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