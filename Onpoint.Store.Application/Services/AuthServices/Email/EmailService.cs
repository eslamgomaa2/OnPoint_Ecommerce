using BuildingBlocks.Common.Helpers;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;


namespace Onpoint.Store.Application.Services.AuthServices.Email
{

    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger)
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.EmailFrom));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = body
                };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                smtp.CheckCertificateRevocation = false;
                // smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;


                await smtp.ConnectAsync(_mailSettings.SmtpHost, _mailSettings.SmtpPort, SecureSocketOptions.StartTls);


                await smtp.AuthenticateAsync(_mailSettings.SmtpUser, _mailSettings.SmtpPassword);


                await smtp.SendAsync(email);

                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email successfully sent to: {ToEmail}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sending email to {ToEmail} failed due to an error in the SMTP server", toEmail);
                throw new InvalidOperationException("Verification email failed, please try again later.");
            }
        }
    }
}
