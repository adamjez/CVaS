using System.Collections.Generic;
using CVaS.BL.DTO;
using CVaS.Web.Models.AccountViewModels;

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
        public List<RuleDTO> Rules { get; set; }
        public RuleViewModel NewRule { get; set; }
        public StatsDTO Stats { get; set; }
    }
}