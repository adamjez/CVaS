using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CVaS.Shared.Core.Provider;
using CVaS.Shared.Providers;
using CVaS.Shared.Repositories;

namespace CVaS.BL.Services.ArgumentTranslator
{
    /// <summary>
    /// TODO: Refactor!
    /// </summary>
    public class BaseArgumentTranslator : IArgumentTranslator
    {
        private const string LocalFileScheme = "local://";

        private readonly IUnitOfWorkProvider _unitOfWorkProvider;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly FileRepository _fileRepository;

        public BaseArgumentTranslator(IUnitOfWorkProvider unitOfWorkProvider, ICurrentUserProvider currentUserProvider,
            FileRepository fileRepository)
        {
            _unitOfWorkProvider = unitOfWorkProvider;
            _currentUserProvider = currentUserProvider;
            _fileRepository = fileRepository;
        }

        public async Task<List<string>> ProcessAsync(IEnumerable<object> array)
        {
            var args = new List<string>();
            foreach (var argument in array)
            {
                args.AddRange(await ProcessSimpleType(argument));

            }

            return args;
        }

        private async Task<IEnumerable<string>> ProcessSimpleType(object arg)
        {
            var typeInfo = arg.GetType().GetTypeInfo();
            if (arg is string)
            {
                return new[] { await ProccessStringAsync((string)arg)};
            }
            else if (arg is float)
            {
                return new[] { ((float)arg).ToString(CultureInfo.InvariantCulture)};
            }
            else if (arg is double)
            {
                return new[] { ((double)arg).ToString(CultureInfo.InvariantCulture)};
            }
            else if (typeInfo.IsPrimitive)
            {
                return new[] { arg.ToString() };
            }
            else if (arg is Dictionary<string, object>)
            {
                return await ProcessDictionary(arg);
            }
            else
            {
                throw new ArgumentException(nameof(arg), "Unknown type for arg: " + arg.GetType());
            }
        }

        private async Task<List<string>> ProcessDictionary(object arg)
        {
            var arguments = new List<string>();
            foreach (var dictValue in (Dictionary<string, object>)arg)
            {
                var builder = new StringBuilder();

                builder.Append('-', dictValue.Key.Length == 1 ? 1 : 2);
                builder.Append(dictValue.Key);
                builder.Append(dictValue.Key.Length == 1 ? ' ' : '=');
                builder.Append((await ProcessSimpleType(dictValue.Value)).FirstOrDefault());

                arguments.Add(builder.ToString());
            }

            return arguments;
        }

        private async Task<string> ProccessStringAsync(string arg)
        {
            if (arg.StartsWith(LocalFileScheme))
            {
                using (_unitOfWorkProvider.Create())
                {
                    var argEntity = await _fileRepository.GetById(int.Parse(arg.Substring(LocalFileScheme.Length)));

                    if (argEntity.UserId != _currentUserProvider.Id)
                    {
                        throw new UnauthorizedAccessException();
                    }

                    return argEntity.Path;
                }
            }
            else
            {
                // Escape string
                return "\"" + arg + "\"";
            }
        }
    }
}