// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
namespace gadifff.Services
{
    public enum PasswordResetEmailStatus
    {
        Sent = 0,
        EmailNotFound = 1,
        InvalidRequest = 2,
        SendFailed = 3
    }
}
