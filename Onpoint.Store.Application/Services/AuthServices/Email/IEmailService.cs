namespace Onpoint.Store.Application.Services.AuthServices.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
