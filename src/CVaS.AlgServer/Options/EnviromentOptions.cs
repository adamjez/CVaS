using System;
using System.Collections;
using System.IO;

namespace CVaS.AlgServer.Options
{
    public class EnviromentOptions : IEnviromentOptions
    {
        private readonly IDictionary _enviromentVariables;
        private EnviromentOptions(IDictionary enviromentVariables)
        {
            _enviromentVariables = enviromentVariables;
        }

        public string EnvironmentName => (string)_enviromentVariables["NETCORE_ENVIRONMENT"];
        public string ContentRootPath => Directory.GetCurrentDirectory();

        public static IEnviromentOptions GetEnviromentOptions()
        {
            return new EnviromentOptions(Environment.GetEnvironmentVariables());
        }
    }
}