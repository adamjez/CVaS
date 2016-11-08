using System;
using System.Text;
using System.Threading.Tasks;
using CVaS.DAL.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Identity;

namespace CVaS.Web.Authentication
{
    public class CustomAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
    {
        private const string Scheme = "Basic";
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public CustomAuthenticationHandler(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string authHeader = Context.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith(Scheme))
            {
                //Extract credentials
                string encodedUsernamePassword = authHeader.Substring(Scheme.Length).Trim();
                string usernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword)); 

                int seperatorIndex = usernamePassword.IndexOf(':');

                if (seperatorIndex == -1)
                {
                    return AuthenticateResult.Fail("Bad format of basic authentication");
                }

                var username = usernamePassword.Substring(0, seperatorIndex);
                var password = usernamePassword.Substring(seperatorIndex + 1);

                var user = await _userManager.FindByNameAsync(username);
                if (user != null && await _userManager.CheckPasswordAsync(user, password))
                {
                    var principals = await _signInManager.CreateUserPrincipalAsync(user);
                    return AuthenticateResult.Success(
                        new AuthenticationTicket(principals, new AuthenticationProperties(), Scheme));
                }

                return AuthenticateResult.Fail("bad username or password");
            }

            return AuthenticateResult.Fail("missing authentication header or bad authorization type");
        }
    }
}
