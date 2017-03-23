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
    /// <summary>
    /// TODO: Refactor!
    /// </summary>
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

            if (arg is string)
            {
                return new[] { ProccessString((string)arg)};
            }
            else if (arg is float)
            {
                return new[] { new GenericArgument<float>((float)arg)};
            }
            else if (arg is double)
            {
                return new[] { new GenericArgument<double>((double)arg)};
            }
            else if (arg is int)
            {
                return new[] { new GenericArgument<int>((int)arg) };
            }
            else if (typeInfo.IsPrimitive)
            {
                return new[] { new GenericArgument<string>(arg.ToString()) };
            }
            else if (arg is Dictionary<string, object>)
            {
                return ProcessDictionary(arg);
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
                var builder = new StringBuilder();

                builder.Append('-', dictValue.Key.Length == 1 ? 1 : 2);
                builder.Append(dictValue.Key);
                builder.Append(dictValue.Key.Length == 1 ? ' ' : '=');
                builder.Append(ProcessSimpleType(dictValue.Value).FirstOrDefault());

                arguments.Add(new GenericArgument<string>(builder.ToString()));
            }

            return arguments;
        }

        private Argument ProccessString(string arg)
        {
            if (arg.StartsWith(LocalFileScheme))
            {
                int fileId;

                if (!int.TryParse(arg.Substring(LocalFileScheme.Length), out fileId))
                {
                    throw new ArgumentMalformedException($"File id in argument: \"{arg}\" is malformed");
                }

                return new FileArgument(fileId);
            }
            else
            {
                return new StringArgument(arg);
            }
        }
    }
}