using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using CVaS.Shared.Exceptions;
using CVaS.Shared.Services.Argument;

namespace CVaS.BL.Services.ArgumentTranslator
{
    public class BasicArgumentTranslator : IArgumentTranslator
    {
        private const string LocalFileScheme = "local://";

        public List<Argument> Process(IEnumerable<object> array)
        {
            var args = new List<Argument>();
            foreach (var argument in array)
            {
                args.AddRange(ProcessSimpleType(argument));
            }

            return args;
        }

        private IEnumerable<Argument> ProcessSimpleType(object arg)
        {
            var typeInfo = arg.GetType().GetTypeInfo();

            switch (arg)
            {
                case string @string:
                    return new[] { ProccessString(@string) };

                case float @float:
                    return new[] { new GenericArgument<float>(@float) };

                case double @double:
                    return new[] { new GenericArgument<double>(@double) };

                case int @int:
                    return new[] { new GenericArgument<int>(@int) };

                case Dictionary<string, object> @dict:
                    return ProcessDictionary(@dict);
            }

            if (typeInfo.IsPrimitive)
            {
                return new[] { new GenericArgument<string>(arg.ToString()) };
            }
            else
            {
                throw new ArgumentException(nameof(arg), "Unknown type for arg: " + arg.GetType());
            }
        }

        private List<Argument> ProcessDictionary(object arg)
        {
            var arguments = new List<Argument>();
            foreach (var dictValue in (Dictionary<string, object>)arg)
            {
                arguments.Add(
                    new KeyValueArgument(
                        dictValue.Key, 
                        ProcessSimpleType(dictValue.Value).FirstOrDefault()
                        )
                    );
            }

            return arguments;
        }

        private Argument ProccessString(string arg)
        {
            if (arg.StartsWith(LocalFileScheme))
            {
                if (Guid.TryParse(arg.Substring(LocalFileScheme.Length), out Guid fileId))
                {
                    return new FileArgument(fileId);
                }

                throw new ArgumentMalformedException($"File id in argument: \"{arg}\" is malformed");
            }
            else
            {
                return new StringArgument(arg);
            }
        }
    }
}