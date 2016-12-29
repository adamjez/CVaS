using Microsoft.Extensions.Configuration;

namespace CVaS.BL.Services.Interpreter
{
    public interface IInterpreterResolver
    {
        string Resolve(string extension);
    }

    public class ConfigInterpreterResolver : IInterpreterResolver
    {
        private static string ConfigurationKey = "Interpreters";
        private readonly IConfigurationRoot _configuration;

        public ConfigInterpreterResolver(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        public string Resolve(string extension)
        {
            return _configuration.GetSection(ConfigurationKey)[extension];
        }
    }
}