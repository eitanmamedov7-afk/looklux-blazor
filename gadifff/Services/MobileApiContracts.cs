// SEARCH INDEX
// API, MOBILE, CONTRACT, REQUEST, RESPONSE, LOGIN, REGISTER, PASSWORD, DASHBOARD, GARMENT, MATCH, OUTFIT, ADMIN
//
// Topic: MOBILE API CONTRACTS
// Purpose: Defines request/response shapes used between MAUI and the Blazor backend API.
// Search keywords: API MOBILE CONTRACT REQUEST RESPONSE LOGIN REGISTER PASSWORD DASHBOARD GARMENT MATCH OUTFIT ADMIN
// When to use it: Show this when explaining what data MAUI sends to or receives from Program.cs endpoints.
// Important notes: These records are transport DTOs, not database table classes.

using Models;

namespace gadifff.Services;

// Role in project:
// Request/response contracts for the MAUI app API.
// These shapes keep mobile login, dashboard, closet, upload, matching, and admin screens aligned with the web backend.

// Request sent by the MAUI app when a user signs in through the mobile API.
public record MobileAuthRequest(string Email, string Password);

// Request sent by the MAUI app when a customer registers from mobile.
public record MobileRegisterRequest(string FullName, string Email, string Password);

// Standard mobile auth response used by login and register.
public record MobileAuthResponse(bool Success, string Message, MobileUserDto? User);

// Forgot/reset password contracts used by the MAUI app.
public record MobileForgotPasswordRequest(string Email, string? OriginBaseUrl);

public record MobileResetPasswordRequest(string Token, string NewPassword);

// Simple success/message response used by mobile endpoints that do not need a complex payload.
public record MobileBasicResponse(bool Success, string Message);

// Dashboard data shown on the MAUI home/admin screen.
public record MobileDashboardResponse(
    bool IsAdmin,
    int CustomerCount,
    int AdminCount,
    int GarmentCount,
    int OutfitCount,
    IReadOnlyList<MobileUsagePoint> Usage);

public record MobileUsagePoint(DateTime Day, int UsersCreated, int GarmentsAdded, int OutfitsSaved);

// Safe user shape returned to the MAUI app; it does not expose PasswordHash.
public record MobileUserDto(string UserId, string Email, string FullName, string Role, DateTime CreatedAt)
{
    // Converts the full User model into the safe mobile response shape.
    // Project process affected: mobile login/register/admin user list.
    public static MobileUserDto FromUser(User user) =>
        new(user.UserId, user.Email, user.FullName, user.Role, user.CreatedAt);
}

// Admin-created user request from the MAUI admin screen.
public record MobileUserCreateRequest(
    string ActorUserId,
    string FullName,
    string Email,
    string Password,
    string Role);

// Admin user update request from the MAUI admin screen.
public record MobileUserUpdateRequest(
    string ActorUserId,
    string FullName,
    string Email,
    string Role);

// Mobile garment upload request.
// ImageBase64 carries the image bytes, and AnalyzeWithAi controls whether Gemini extracts garment features.
public record MobileGarmentCreateRequest(
    string UserId,
    string ImageBase64,
    string MimeType,
    string FileName,
    string? Type,
    bool AnalyzeWithAi,
    string? Color = null,
    string? ColorSecondary = null,
    string? Pattern = null,
    string? StyleCategory = null,
    string? Season = null,
    string? Occasion = null,
    int? FormalityLevel = null,
    string? StyleTags = null,
    string? Fit = null,
    string? Material = null,
    string? Sleeve = null,
    string? Length = null,
    string? Brand = null);

// Mobile garment upload result.
// RequiresManualType is kept for API compatibility, though the mobile UI now asks the user to retry clearer photos.
public record MobileGarmentCreateResponse(
    bool Success,
    string Message,
    bool RequiresManualType,
    Garment? Garment);

// Request for running recommendations from the MAUI app.
public record MobileMatchRequest(
    string UserId,
    string[]? SeedGarmentIds,
    bool AllowNoSeed,
    string? UserRequest);

// Request for saving one recommendation as an outfit from the MAUI app.
public record MobileSaveMatchRequest(
    string UserId,
    ScoredCombination Recommendation,
    string[]? SeedGarmentIds);

// Shared delete request shape for endpoints that only need the acting user id.
public record MobileDeleteRequest(string UserId);
