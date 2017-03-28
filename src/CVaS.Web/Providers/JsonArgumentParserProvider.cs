using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace CVaS.Web.Providers
{
    /// <summary>
    /// Parses JToken with defined structure
    /// Arguments can be only one layer deep, so
    /// JArray can not contains another JArray on so on
    /// </summary>
    public class JsonArgumentParser : IArgumentParser
    {
        public bool CanParse(object arguments)
        {
            return arguments is JToken;
        }

        public List<object> Parse(object arguments)
        {
            return CustomArgumentParser((JToken) arguments);
        }

        private static List<object> CustomArgumentParser(JToken root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            var array = new List<object>();

            if (root.Type == JTokenType.Array)
            {
                array.AddRange(root.Children().Select(token => ParseValues(token, true)));
            }
            else
            {
                array.Add(ParseValues(root, true));
            }

            return array;
        }

        private static object ParseValues(JToken token, bool topLevel)
        {
            switch (token.Type)
            {
                case JTokenType.String:
                    return token.Value<string>();
                case JTokenType.Integer:
                    return token.Value<int>();
                case JTokenType.Float:
                    return token.Value<float>();
                case JTokenType.Boolean:
                    return token.Value<bool>();
                case JTokenType.Object when (topLevel):
                    var dict = new Dictionary<string, object>();
                    foreach (var property in token.Value<JObject>())
                    {
                        dict.Add(property.Key, ParseValues(property.Value, false));
                    }

                    return dict;
                default:
                    throw new ArgumentException(nameof(token));
            }
        }
    }
}