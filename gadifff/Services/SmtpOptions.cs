



namespace gadifff.Services
{
    // Strongly-typed configuration for SMTP email sending.
    // Program.cs binds appsettings/user-secrets/environment settings into this class.
    public class SmtpOptions
    {
        // Name of the appsettings section: "Smtp".
        public const string SectionName = "Smtp";

        // SMTP server connection details.
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public bool UseDefaultCredentials { get; set; }

        // Login credentials for providers that require authentication.
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Sender identity shown in reset-password emails.
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "Closet Support";
    }
}
