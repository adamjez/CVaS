namespace CVaS.Web.Models
{
    public class LoginPanelViewModel
    {
        public bool SignedIn { get; set; }

        public string CurrentUserName { get; set; }
        public bool IsAdmin { get; set; }
    }
}