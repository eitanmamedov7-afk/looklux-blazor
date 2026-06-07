



using System.Threading.Tasks;

namespace gadifff.Services
{
    // SEARCH INDEX
    // EMAIL, SMTP, PASSWORD, RESET, SEND
    //
    // Contract for sending emails.
    // The project uses it for password reset emails, and SmtpEmailSender is the current implementation.
    public interface IEmailSender
    {
        // Sends an HTML email with optional plain-text fallback content.
        // FLOW_PASSWORD_RESET_WEB_05: AuthService calls this interface for the web reset email.
        // Dependency injection routes the call to SmtpEmailSender.SendAsync.
        // FLOW_PASSWORD_RESET_MOBILE_07: AuthService calls this same interface for MAUI-started reset emails.
        // Dependency injection routes the call to SmtpEmailSender.SendAsync.
        Task<bool> SendAsync(string toEmail, string? toName, string subject, string htmlBody, string? textBody = null);
    }
}
