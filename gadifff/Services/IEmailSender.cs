// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
using System.Threading.Tasks;

namespace gadifff.Services
{
    public interface IEmailSender
    {
        Task<bool> SendAsync(string toEmail, string? toName, string subject, string htmlBody, string? textBody = null);
    }
}
