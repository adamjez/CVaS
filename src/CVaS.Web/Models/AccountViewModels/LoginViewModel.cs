using System.ComponentModel.DataAnnotations;

namespace CVaS.Web.Models.AccountViewModels
{
    public class LoginViewModel : LayoutViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}