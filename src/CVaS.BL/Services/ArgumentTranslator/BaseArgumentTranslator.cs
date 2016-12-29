using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CVaS.BL.Core.Provider;
using CVaS.BL.Providers;
using CVaS.BL.Repositories;

namespace CVaS.BL.Services.ArgumentTranslator
{
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

        public async Task<string> ProcessAsync(object arg)
        {
            if (arg is IEnumerable<object>)
            {
                return await ProcessArray((IEnumerable<object>)arg);
            }
            else
            {
                return await ProcessSimpleType(arg);
            }
        }

        private async Task<string> ProcessArray(IEnumerable<object> array)
        {
            var builder = new StringBuilder();
            foreach (var argument in array)
            {
                builder.Append(await ProcessSimpleType(argument));
                builder.Append(' ');
            }

            return builder.ToString();
        }

        private async Task<string> ProcessSimpleType(object arg)
        {
            var typeInfo = arg.GetType().GetTypeInfo();
            if (arg is string)
            {
                return await ProccessStringAsync((string)arg);
            }
            else if (arg is float)
            {
                return ((float)arg).ToString(CultureInfo.InvariantCulture);
            }
            else if (arg is double)
            {
                return ((double)arg).ToString(CultureInfo.InvariantCulture);
            }
            else if (typeInfo.IsPrimitive)
            {
                return arg.ToString();
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

        private async Task<string> ProcessDictionary(object arg)
        {
            var builder = new StringBuilder();
            foreach (var dictValue in (Dictionary<string, object>)arg)
            {
                builder.Append('-', dictValue.Key.Length == 1 ? 1 : 2);
                builder.Append(dictValue.Key);
                builder.Append('=');
                builder.Append(await ProcessSimpleType(dictValue.Value));
                builder.Append(' ');
            }
            return builder.ToString();
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