using System.Collections.Generic;
using System.Reflection;

namespace CVaS.Web.Providers
{
    /// <summary>
    /// Parses primite arguments like bool, int, float
    /// and string
    /// </summary>
    public class PrimitiveArgumentParser : IArgumentParser
    {
        public bool CanParse(object argument)
        {
            return argument == null 
                || argument.GetType().GetTypeInfo().IsPrimitive 
                || argument is string;
        }

        public List<object> Parse(object argument)
        {
            var result =  new List<object>();

            if(argument != null)
                result.Add(argument);

            return result;
        }
    }
}