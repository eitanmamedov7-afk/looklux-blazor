// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
using System;
using System.Threading.Tasks;
using Models;

namespace gadifff.Services
{
    public interface IAuthService
    {
        event Action? AuthStateChanged;

        Task<bool> LoginAsync(string email, string password);
        Task LogoutAsync();

        // FullName קודם ואז Email
        Task<bool> RegisterAsync(string fullName, string email, string password, string role);

        Task<PasswordResetEmailStatus> SendPasswordResetEmailAsync(string email, string originBaseUrl);
        Task<bool> ResetPasswordAsync(string token, string newPassword);

        Task<User?> CurrentUserAsync();
    }
}
