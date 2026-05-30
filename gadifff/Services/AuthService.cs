using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBL;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Models;
using BCryptNet = BCrypt.Net.BCrypt;
namespace gadifff.Services
{
    // Role in project:
    // Central authentication/account service used by Login/Register/Forgot/Reset pages.
    // It keeps the current signed-in user in memory and exposes auth state changes to UI pages.
    public class AuthService : IAuthService
    {
        private readonly UserDB _userDb;
        private readonly IEmailSender _emailSender;
        private readonly IDataProtector _passwordResetProtector;
        private readonly ILogger<AuthService> _logger;

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
        public async Task<bool> LoginAsync(string email, string password)
        {
            email = (email ?? string.Empty).Trim().ToLowerInvariant();
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
        public Task LogoutAsync()
        {
            _currentUser = null;
            _lastSignedInUser = null;
            NotifyAuthChanged();
            return Task.CompletedTask;
        }
        // Registration flow used by Register.razor:
        // creates a new customer user and signs them in immediately.
        public async Task<bool> RegisterAsync(string fullName, string email, string password, string role)
        {
            email = (email ?? string.Empty).Trim().ToLowerInvariant();
            var normalizedRole = "customer";
            var existing = await _userDb.GetByEmailAsync(email);
            if (existing.Any())
                return false;
            var user = new User
            {
                UserId = Guid.NewGuid().ToString(),
                Email = email,
                FullName = fullName?.Trim() ?? string.Empty,
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
        public async Task<PasswordResetEmailStatus> SendPasswordResetEmailAsync(string email, string originBaseUrl)
        {
            email = (email ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(originBaseUrl))
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
        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            token = token?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
                return false;
            if (newPassword.Length < 6)
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
        // Used by pages (for example Closet/Outfits) to gate access and resolve user context.
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

