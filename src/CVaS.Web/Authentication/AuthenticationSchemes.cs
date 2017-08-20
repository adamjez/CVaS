namespace CVaS.Web.Authentication
{
    public static class AuthenticationSchemes
    {
        public const string WebCookie = "Identity.Application";
        public const string ApiKey = "ApiKeyScheme";
        public const string Both = "ApiKeyScheme,Identity.Application";
    }
}
