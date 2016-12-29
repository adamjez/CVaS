using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace CVaS.Web.Helpers
{
    public class JsonArgumentParserProvider : IArgumentParserProvider
    {
        public bool CanParse(object arguments)
        {
            return arguments is JToken;
        }

        public List<object> Parse(object arguments)
        {
            return JsonCustomArgumentParser.CustomArgumentParser((JToken) arguments);
        }
    }

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