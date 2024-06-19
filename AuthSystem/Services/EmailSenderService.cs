using AuthSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Security.Policy;

namespace AuthSystem.Services
{
    public class EmailSenderService : IEmailSender
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly SendGridSettings _sendGridSettings;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmailSenderService(ISendGridClient sendGridClient, IOptions<SendGridSettings> sendGridSettings)
        {
            _sendGridClient = sendGridClient;
            _sendGridSettings = sendGridSettings.Value;

        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_sendGridSettings.FromEmail, _sendGridSettings.EmailName),
                Subject = subject,
                HtmlContent = htmlMessage
            };
            msg.AddTo(email);
            await _sendGridClient.SendEmailAsync(msg);
        }
    }
}
