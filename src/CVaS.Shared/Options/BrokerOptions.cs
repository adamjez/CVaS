namespace CVaS.Shared.Options
{
    public class BrokerOptions
    {
        public string Hostname { get; set; } = "localhost";
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string Vhost { get; set; } = "/";
    }
}