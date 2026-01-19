using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;
namespace CapManagement.Server.Services
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public SendGridEmailSender(IConfiguration config)
        {
            _config = config;
        }


        /// <summary>
        /// Sends an email using the SendGrid email service.
        /// </summary>
        /// <param name="email">Recipient email address.</param>
        /// <param name="subject">Email subject line.</param>
        /// <param name="htmlMessage">HTML content of the email body.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the email or subject is invalid.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown when SendGrid fails to send the email.
        /// </exception>
        /// <remarks>
        /// This method uses SendGrid API credentials from application configuration
        /// and sends an HTML email using the configured sender address.
        /// </remarks>
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var apiKey = _config["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);

            var from = new EmailAddress(
                _config["SendGrid:FromEmail"],
                _config["SendGrid:FromName"]);

            var to = new EmailAddress(email);

            var msg = MailHelper.CreateSingleEmail(
                from,
                to,
                subject,
                plainTextContent: null,
                htmlContent: htmlMessage);

            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"SendGrid failed: {response.StatusCode}");
            }
        }
    }
}
