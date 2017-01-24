using System.ComponentModel.DataAnnotations;

namespace CVaS.Web.Models.AccountViewModels
{
    public class ForgotPasswordViewModel : LayoutViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}