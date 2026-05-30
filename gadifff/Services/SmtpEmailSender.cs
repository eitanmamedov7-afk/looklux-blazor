// מה הקובץ עושה: הקובץ מרכז חלק מהמערכת ומשתתף בהפעלת הפרויקט.
// למה הקובץ נדרש: הוא נדרש כדי שהחלק הזה בפרויקט יפעל בצורה ברורה ומסודרת.
// לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר למסכים, לשירותים, למודלים ולשכבת הדיבי לפי השימוש שלו.
// איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים לקבצים שמזמנים את הקוד הזה או לקבצים שהוא מזמן.

// הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
// הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
// לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר לדפי בלייזור, למודלם, לדיבי ולשירותים נוספים.
// איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים בדפים שמזריקים את השירות ובקבצי הדיבי שהשירות קורא להם.



// ייבוא ספריות שמספקות מחלקות, ממשקים ופעולות שהקובץ צריך כדי לעבוד.
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

// הגדרת מרחו שמות שממקם את הקובץ בטבקת הפרויקט המטאימה.
namespace gadifff.Services
{
    // הגדרת מבנה מרכזי שמרכז נתונים או פעוליות עובר החלק הזה בפרויקט.
    public class SmtpEmailSender : IEmailSender
    {
        // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
        private readonly IOptionsMonitor<SmtpOptions> _smtpOptions;
        // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
        private readonly ILogger<SmtpEmailSender> _logger;

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public SmtpEmailSender(IOptionsMonitor<SmtpOptions> smtpOptions, ILogger<SmtpEmailSender> logger)
        {
            _smtpOptions = smtpOptions;
            _logger = logger;
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<bool> SendAsync(string toEmail, string? toName, string subject, string htmlBody, string? textBody = null)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var options = _smtpOptions.CurrentValue;

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (string.IsNullOrWhiteSpace(toEmail) ||
                string.IsNullOrWhiteSpace(options.Host) ||
                string.IsNullOrWhiteSpace(options.FromEmail))
            {
                _logger.LogWarning("SMTP is not configured correctly. Host/FromEmail/ToEmail must be provided.");
                // החזרת התוצאה אל הקוד שקרא לפעולה.
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
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
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

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (!options.UseDefaultCredentials && !string.IsNullOrWhiteSpace(options.Username))
            {
                smtp.Credentials = new NetworkCredential(options.Username.Trim(), options.Password ?? string.Empty);
            }

            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                await smtp.SendMailAsync(message);
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return true;
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}.", toEmail);
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return false;
            }
        }
    }
}
