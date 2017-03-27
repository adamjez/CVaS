using CVaS.Shared.Providers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Linq;

namespace CVaS.Web.TagHelpers
{
    public class AuthorizeTagHelper : TagHelper
    {
        private readonly ICurrentUserProvider _currentUserProvider;

        /// <summary>
        /// Roles separeted with comma
        /// </summary>
        public string Roles { get; set; }

        public AuthorizeTagHelper(ICurrentUserProvider currentUserProvider)
        {
            _currentUserProvider = currentUserProvider;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(Roles))
            {
                return;
            }

            var rolesCollection = Roles.Split(',');

            var inAllowedRole = rolesCollection.Any(role => _currentUserProvider.IsInRole(role));
            if (!inAllowedRole)
            {
                output.SuppressOutput();
            }
            else
            {
                output.TagName = string.Empty;
            }

        }
    }
}
