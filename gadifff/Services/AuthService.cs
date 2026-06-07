using System;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using DBL;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Models;
using BCryptNet = BCrypt.Net.BCrypt;
namespace gadifff.Services
{
    // SEARCH INDEX
    // LOGIN, REGISTER, PASSWORD, RESET, EMAIL, COOKIE, VALIDATE, USER, AUTH, SESSION
    //
    // Topic: AUTHENTICATION SERVICE
    // Purpose: Handles web login/register/logout state plus forgot-password and reset-password logic.
    // Search keywords: LOGIN REGISTER PASSWORD RESET EMAIL COOKIE VALIDATE USER AUTH SESSION
    // When to use it: Show this when explaining user authentication or password recovery.
    // Important notes: Web session state is kept for Blazor UI; mobile auth endpoints call related logic from Program.cs.
    //
    // SECTION: AUTH LOGIN REGISTER PASSWORD
    // Role in project:
    // Central authentication/account service used by Login/Register/Forgot/Reset pages.
    // It keeps the current signed-in user in memory and exposes auth state changes to UI pages.
    public class AuthService : IAuthService
    {
        private readonly UserDB _userDb;
        private readonly IEmailSender _emailSender;
        private readonly IDataProtector _passwordResetProtector;
        private readonly ILogger<AuthService> _logger;

        // Topic: Web signed-in user state
        // Purpose: Keeps the logged-in web user available to Blazor pages after LoginAsync succeeds.
        // Search keywords: LOGIN AUTH SESSION USER CURRENT COOKIE
        // When to use it: Show this when explaining where the web project remembers the active user.
        // Important notes: This is in-memory Blazor service state, with _lastSignedInUser as the current project fallback.
        // FLOW_AUTH_STATE_WEB_01: AuthService stores the signed-in user in _currentUser after successful login/register.
        // This file is involved because pages ask AuthService.CurrentUserAsync before loading closet/outfit/admin data.
        private User? _currentUser;
        private static User? _lastSignedInUser;
        private static readonly TimeSpan PasswordResetTokenLifetime = TimeSpan.FromHours(2);
        public event Action? AuthStateChanged;

        public AuthService(
            UserDB userDb,
            IEmailSender emailSender,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<AuthService> logger)
        {
            _userDb = userDb;
            _emailSender = emailSender;
            _passwordResetProtector = dataProtectionProvider.CreateProtector("gadifff.auth.password-reset.v1");
            _logger = logger;
        }
        // Notifies subscribed UI components (nav/menu/pages) to refresh auth-dependent state.
        private void NotifyAuthChanged()
        {
            AuthStateChanged?.Invoke();
        }
        // Login flow used by Login.razor:
        // validates credentials against UserDB and sets _currentUser for the active session.
        // FLOW_LOGIN_WEB_03: AuthService.LoginAsync receives web credentials from Login.razor.
        // This file is involved because it owns web auth state; next step is UserDB.GetByEmailAsync and BCrypt validation.
        // FLOW_AUTH_STATE_WEB_02: After BCrypt succeeds, LoginAsync saves the User object in _currentUser/_lastSignedInUser.
        // This file is involved because this is the exact place the web user becomes "logged in"; next step is UI pages calling CurrentUserAsync.
        public async Task<bool> LoginAsync(string email, string password)
        {
            email = (email ?? string.Empty).Trim().ToLowerInvariant();
            // VALIDATION_EMAIL / VALIDATION_PASSWORD: AuthService rejects malformed login input before UserDB lookup.
            if (!IsValidEmail(email) || string.IsNullOrWhiteSpace(password))
                return false;

            var list = await _userDb.GetByEmailAsync(email);
            var user = list.FirstOrDefault();
            if (user == null)
                return false;
            if (string.IsNullOrEmpty(user.PasswordHash) ||
                !BCryptNet.Verify(password, user.PasswordHash))
            {
                return false;
            }

            _currentUser = user;
            _lastSignedInUser = user;
            NotifyAuthChanged();
            return true;
        }
        // Logout flow used by Logout page/menu actions.
        // FLOW_AUTH_STATE_WEB_05: LogoutAsync clears _currentUser/_lastSignedInUser so pages stop treating the user as signed in.
        // This file is involved because logout must clear the same state that login created.
        public Task LogoutAsync()
        {
            _currentUser = null;
            _lastSignedInUser = null;
            NotifyAuthChanged();
            return Task.CompletedTask;
        }
        // Registration flow used by Register.razor:
        // creates a new customer user and signs them in immediately.
        // FLOW_REGISTER_WEB_03: AuthService.RegisterAsync checks duplicate email, hashes password, and creates a customer user.
        // This file is involved because registration owns account creation rules; next step is UserDB.CreateAsync.
        public async Task<bool> RegisterAsync(string fullName, string email, string password, string role)
        {
            email = (email ?? string.Empty).Trim().ToLowerInvariant();
            // VALIDATION_NAME / VALIDATION_EMAIL / VALIDATION_PASSWORD: AuthService validates web registration before hashing/inserting.
            if (!FullNameRules.TryNormalize(fullName, out fullName) || !IsValidEmail(email) || !IsValidPassword(password))
                return false;

            var normalizedRole = "customer";
            var existing = await _userDb.GetByEmailAsync(email);
            if (existing.Any())
                return false;
            var user = new User
            {
                UserId = Guid.NewGuid().ToString(),
                Email = email,
                FullName = fullName,
                Role = normalizedRole,
                PasswordHash = BCryptNet.HashPassword(password),
                CreatedAt = DateTime.UtcNow
            };
            var inserted = await _userDb.CreateAsync(user);
            if (inserted <= 0)
                return false;

            _currentUser = user;
            _lastSignedInUser = user;
            NotifyAuthChanged();
            return true;
        }
        // Forgot-password flow:
        // creates a protected, expiring reset token and sends reset link by email.
        // FLOW_PASSWORD_RESET_WEB_04: AuthService creates the protected token and browser reset link after UserDB finds the account.
        // This file is involved because web reset security is centralized here; next step is IEmailSender.SendAsync.
        // FLOW_PASSWORD_RESET_MOBILE_06: AuthService creates the same protected token/link for the MAUI-started reset.
        // This file is involved because MAUI reuses the web reset security; next step is IEmailSender.SendAsync.
        public async Task<PasswordResetEmailStatus> SendPasswordResetEmailAsync(string email, string originBaseUrl)
        {
            email = (email ?? string.Empty).Trim().ToLowerInvariant();
            // VALIDATION_EMAIL / VALIDATION_RESET_ORIGIN: password-reset email needs valid email and reset origin.
            if (!IsValidEmail(email) || string.IsNullOrWhiteSpace(originBaseUrl))
                return PasswordResetEmailStatus.InvalidRequest;
            var user = (await _userDb.GetByEmailAsync(email)).FirstOrDefault();
            if (user == null)
                return PasswordResetEmailStatus.EmailNotFound;
            var expiresAtUtc = DateTime.UtcNow.Add(PasswordResetTokenLifetime);
            var payload = $"{user.UserId}|{expiresAtUtc.Ticks}";
            var protectedPayload = _passwordResetProtector.Protect(payload);
            var token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(protectedPayload));
            var baseUri = originBaseUrl.Trim().TrimEnd('/');
            var link = $"{baseUri}/reset-password?token={Uri.EscapeDataString(token)}";
            var safeName = string.IsNullOrWhiteSpace(user.FullName) ? "there" : user.FullName.Trim();
            var subject = "Reset your password";
            var htmlBody = $"""
                            <p>Hi {System.Net.WebUtility.HtmlEncode(safeName)},</p>
                            <p>We received a request to reset your password.</p>
                            <p><a href="{System.Net.WebUtility.HtmlEncode(link)}">Click here to reset your password</a></p>
                            <p>This link expires in 2 hours.</p>
                            <p>If you didn't request this, you can ignore this email.</p>
                            """;
            var textBody = $"Hi {safeName},\n\nWe received a request to reset your password.\nReset link: {link}\n\nThis link expires in 2 hours.\nIf you didn't request this, you can ignore this email.";
            var sent = await _emailSender.SendAsync(user.Email, user.FullName, subject, htmlBody, textBody);
            if (!sent)
            {
                _logger.LogWarning("Failed to send password reset email to {Email}.", user.Email);
                return PasswordResetEmailStatus.SendFailed;
            }
            return PasswordResetEmailStatus.Sent;
        }
        // Reset-password flow:
        // validates token integrity/expiry and writes a new password hash to UserDB.
        // FLOW_PASSWORD_RESET_WEB_08: AuthService validates the web reset token, checks expiry, and hashes the new password.
        // This file is involved because token security belongs in one shared service; next step is UserDB.UpdatePasswordHashAsync.
        // FLOW_PASSWORD_RESET_MOBILE_09: AuthService validates the reset token even when the email was started from MAUI.
        // This file is involved because the final password change still uses the shared web reset page; next step is UserDB.UpdatePasswordHashAsync.
        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            token = token?.Trim() ?? string.Empty;
            // VALIDATION_RESET_TOKEN / VALIDATION_PASSWORD: reset requires a token and a valid new password.
            if (string.IsNullOrWhiteSpace(token) || !IsValidPassword(newPassword))
                return false;

            string payload;
            try
            {
                var protectedPayload = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
                payload = _passwordResetProtector.Unprotect(protectedPayload);
            }
            catch
            {
                return false;
            }
            var parts = payload.Split('|', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
                return false;
            var userId = parts[0];
            if (string.IsNullOrWhiteSpace(userId) || !long.TryParse(parts[1], out var expiryTicks))
                return false;
            var expiresAtUtc = new DateTime(expiryTicks, DateTimeKind.Utc);
            if (expiresAtUtc < DateTime.UtcNow)
                return false;
            var user = await _userDb.GetByIdAsync(userId);
            if (user == null)
                return false;
            var newHash = BCryptNet.HashPassword(newPassword);
            var updated = await _userDb.UpdatePasswordHashAsync(user.UserId, newHash);
            return updated > 0;
        }

        private static bool IsValidEmail(string? email)
        {
            // VALIDATION_EMAIL: AuthService validates email shape for login/register/reset flows.
            if (string.IsNullOrWhiteSpace(email) || email.Length > 254)
                return false;

            try
            {
                var address = new MailAddress(email.Trim());
                return string.Equals(address.Address, email.Trim(), StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidPassword(string? password)
        {
            // VALIDATION_PASSWORD: AuthService enforces the project password length rule.
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 6 && password.Length <= 128;
        }

        // Used by pages (for example Closet/Outfits) to gate access and resolve user context.
        // FLOW_AUTH_STATE_WEB_04: CurrentUserAsync returns the remembered user to pages/layouts that need auth state.
        // This file is involved because Closet, Outfits, Home, and NavMenu call this before showing user-specific data.
        public Task<User?> CurrentUserAsync()
        {
            if (_currentUser != null)
                return Task.FromResult<User?>(_currentUser);

            if (_lastSignedInUser != null)
                _currentUser = _lastSignedInUser;

            return Task.FromResult(_currentUser);
        }
    }
}

