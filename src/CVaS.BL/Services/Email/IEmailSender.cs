using System.Threading.Tasks;

namespace CVaS.BL.Services.Email
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}