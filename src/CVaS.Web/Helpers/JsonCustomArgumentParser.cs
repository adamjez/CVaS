﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CVaS.Web.Helpers
{
    public static class JsonCustomArgumentParser
    {
        public static List<object> CustomArgumentParser(JToken root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            var array = new List<object>();

            if (root.Type == JTokenType.Array)
            {

                foreach (var token in root.Children())
                {
                    array.Add(ParseValues(token, true));
                }
            }
            else
            {
                array.Add(ParseValues(root, true));
            }

            return array;
        }

        private static object ParseValues(JToken token, bool topLevel)
        {
            if (token.Type == JTokenType.String)
            {
                return token.Value<string>();
            }
            else if (token.Type == JTokenType.Integer)
            {
                return token.Value<int>();
            }
            else if (token.Type == JTokenType.Float)
            {
                return token.Value<float>();
            }
            else if (token.Type == JTokenType.Boolean)
            {
                return token.Value<bool>();
            }
            else if (token.Type == JTokenType.Object && topLevel)
            {
                var dict = new Dictionary<string, object>();
                foreach (var property in token.Value<JObject>())
                {
                    dict.Add(property.Key, ParseValues(property.Value, false));
                }

                return dict;
            }
            else
            {
                throw new ArgumentException(nameof(token));
            }
        }
    }
}