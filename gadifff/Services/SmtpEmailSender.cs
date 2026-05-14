// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace gadifff.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IOptionsMonitor<SmtpOptions> _smtpOptions;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptionsMonitor<SmtpOptions> smtpOptions, ILogger<SmtpEmailSender> logger)
        {
            _smtpOptions = smtpOptions;
            _logger = logger;
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public async Task<bool> SendAsync(string toEmail, string? toName, string subject, string htmlBody, string? textBody = null)
        {
            var options = _smtpOptions.CurrentValue;

            if (string.IsNullOrWhiteSpace(toEmail) ||
                string.IsNullOrWhiteSpace(options.Host) ||
                string.IsNullOrWhiteSpace(options.FromEmail))
            {
                _logger.LogWarning("SMTP is not configured correctly. Host/FromEmail/ToEmail must be provided.");
                return false;
            }

            using var message = new MailMessage
            {
                From = new MailAddress(options.FromEmail.Trim(), options.FromName?.Trim() ?? string.Empty),
                Subject = subject ?? string.Empty,
                Body = htmlBody ?? string.Empty,
                IsBodyHtml = true
            };

            message.To.Add(new MailAddress(toEmail.Trim(), (toName ?? string.Empty).Trim()));
            if (!string.IsNullOrWhiteSpace(textBody))
            {
                message.AlternateViews.Add(
                    AlternateView.CreateAlternateViewFromString(textBody, null, "text/plain"));
            }

            using var smtp = new SmtpClient(options.Host.Trim(), options.Port)
            {
                EnableSsl = options.EnableSsl,
                UseDefaultCredentials = options.UseDefaultCredentials
            };

            if (!options.UseDefaultCredentials && !string.IsNullOrWhiteSpace(options.Username))
            {
                smtp.Credentials = new NetworkCredential(options.Username.Trim(), options.Password ?? string.Empty);
            }

            try
            {
                await smtp.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}.", toEmail);
                return false;
            }
        }
    }
}
