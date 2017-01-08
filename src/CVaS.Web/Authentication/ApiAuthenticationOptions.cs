using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace CVaS.Web.Authentication
{
    public class ApiAuthenticationOptions : AuthenticationOptions, IOptions<ApiAuthenticationOptions>
    {

        /// <summary>
        /// Create an instance of the options initialized with the default values
        /// </summary>
        public ApiAuthenticationOptions()
        {
            HeaderScheme = "ApiKey";
            AutomaticAuthenticate = true;
            AutomaticChallenge = true;
        }

        /// <summary>
        /// Gets or sets the Realm sent in the WWW-Authenticate header.
        /// </summary>
        /// <remarks>
        /// The realm value (case-sensitive), in combination with the canonical root URL 
        /// of the server being accessed, defines the protection space. 
        /// These realms allow the protected resources on a server to be partitioned into a 
        /// set of protection spaces, each with its own authentication scheme and/or 
        /// authorization database. 
        /// </remarks>
 

        public ApiAuthenticationOptions Value => this;

        public string HeaderScheme { get; set; }
    }
}