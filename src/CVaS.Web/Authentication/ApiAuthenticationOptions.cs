using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace CVaS.Web.Authentication
{
    public class ApiAuthenticationOptions : AuthenticationSchemeOptions
    {

        /// <summary>
        /// Create an instance of the options initialized with the default values
        /// </summary>
        public ApiAuthenticationOptions()
        {
            HeaderScheme = "ApiKey";
        }

        public string HeaderScheme { get; set; }

        public string AuthenticationScheme { get; set; }

    }
}
