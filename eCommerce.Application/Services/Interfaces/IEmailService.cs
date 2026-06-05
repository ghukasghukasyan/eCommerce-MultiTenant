namespace eCommerce.Application.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string toEmail, string toName, string confirmationLink);
        Task SendPasswordResetAsync(string toEmail, string toName, string resetLink);
    }
}
