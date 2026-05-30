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
using System.Threading.Tasks;
using Models;

// הגדרת מרחו שמות שממקם את הקובץ בטבקת הפרויקט המטאימה.
namespace gadifff.Services
{
    // הגדרת מבנה מרכזי שמרכז נתונים או פעוליות עובר החלק הזה בפרויקט.
    public interface IAuthService
    {
        event Action? AuthStateChanged;

        Task<bool> LoginAsync(string email, string password);
        Task LogoutAsync();

        Task<bool> RegisterAsync(string fullName, string email, string password, string role);

        Task<PasswordResetEmailStatus> SendPasswordResetEmailAsync(string email, string originBaseUrl);
        Task<bool> ResetPasswordAsync(string token, string newPassword);

        Task<User?> CurrentUserAsync();
    }
}
