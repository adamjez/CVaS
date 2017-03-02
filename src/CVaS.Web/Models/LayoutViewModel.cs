using System.Collections.Generic;
using CVaS.BL.DTO;

namespace CVaS.Web.Models
{
    public class LayoutViewModel
    {
        public bool SignedIn { get; set; }

        public string CurrentUserName { get; set; }

        public string Title { get; set; }

        public string ReturnUrl { get; set; }
    }

    public class AdminSectionViewModel : LayoutViewModel
    {
        public List<AlgorithmStatsListDTO> Algorithms { get; set; }
    }

    public class LoginPanelViewModel
    {
        public bool SignedIn { get; set; }

        public string CurrentUserName { get; set; }
        public bool IsAdmin { get; set; }
    }
}