



using System;
using System.Threading.Tasks;
using Models;

namespace gadifff.Services
{
    // Contract for authentication/account actions used by pages and mobile endpoints.
    // AuthService implements this so UI code depends on a simple interface instead of concrete logic.
    public interface IAuthService
    {
        // UI components subscribe to this so nav/page state refreshes after login/logout.
        event Action? AuthStateChanged;

        // Sign-in, sign-out, and registration actions used by Login/Register/Logout pages.
        Task<bool> LoginAsync(string email, string password);
        Task LogoutAsync();

        Task<bool> RegisterAsync(string fullName, string email, string password, string role);

        // Forgot/reset password actions used by web pages and the MAUI mobile API.
        Task<PasswordResetEmailStatus> SendPasswordResetEmailAsync(string email, string originBaseUrl);
        Task<bool> ResetPasswordAsync(string token, string newPassword);

        // Resolves the active user so protected pages know which closet/outfits to load.
        Task<User?> CurrentUserAsync();
    }
}
