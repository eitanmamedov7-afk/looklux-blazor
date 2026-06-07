



namespace gadifff.Services
{
    // Result values from the forgot-password email flow.
    // ForgotPassword.razor and the mobile API convert these statuses into user-facing messages.
    public enum PasswordResetEmailStatus
    {
        Sent = 0,
        EmailNotFound = 1,
        InvalidRequest = 2,
        SendFailed = 3
    }
}
