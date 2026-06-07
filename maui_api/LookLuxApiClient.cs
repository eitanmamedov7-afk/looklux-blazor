// SEARCH INDEX
// MAUI, MOBILE, API, HTTP, LOGIN, REGISTER, PASSWORD, DASHBOARD, CLOSET, GARMENT, MATCH, OUTFIT, ADMIN, DELETE
//
// Topic: MAUI API CLIENT
// Purpose: Centralizes all HTTP calls from MAUI to the Blazor backend API.
// Search keywords: MAUI MOBILE API HTTP LOGIN REGISTER PASSWORD DASHBOARD CLOSET GARMENT MATCH OUTFIT ADMIN DELETE
// When to use it: Show this when explaining that MAUI uses the backend through REST-style endpoints.
// Important notes: This file mirrors the mobile API endpoints defined in gadifff/Program.cs.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Models;

namespace gadifff.Mobile;

public sealed class LookLuxApiClient
{
    // SECTION: MOBILE HTTP API WRAPPER
    // Topic: HTTP client wrapper
    // Purpose: Stores backend base URL and exposes typed methods for each mobile API route.
    // Search keywords: API HTTP MOBILE CLIENT REQUEST RESPONSE
    // When to use it: Use when tracing a MAUI button click to the backend endpoint.
    // Important notes: Records at the bottom define the request/response data shapes used by MAUI.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http = new();

    public Uri BaseUri { get; private set; }

    public LookLuxApiClient(Uri baseUri)
    {
        BaseUri = NormalizeBaseUri(baseUri);
        _http.BaseAddress = BaseUri;
    }

    public void SetBaseUri(Uri baseUri)
    {
        BaseUri = NormalizeBaseUri(baseUri);
        _http.BaseAddress = BaseUri;
    }

    public Uri BuildAbsoluteUri(string relativePath)
    {
        var path = relativePath.StartsWith("/", StringComparison.Ordinal) ? relativePath[1..] : relativePath;
        return new Uri(BaseUri, path);
    }

    // Topic: Login API call
    // Purpose: Posts MAUI credentials to the backend login endpoint.
    // FLOW_LOGIN_MOBILE_03: LoginAsync sends MAUI credentials to /api/mobile/auth/login.
    // This file is involved because all MAUI backend calls pass through this HTTP client; next step is Program.cs endpoint.
    public async Task<MobileAuthResponse?> LoginAsync(string email, string password) =>
        await PostJsonAsync<MobileAuthRequest, MobileAuthResponse>(
            "api/mobile/auth/login",
            new(email, password));

    // Topic: Register API call
    // Purpose: Posts the mobile register form to the backend account-creation endpoint.
    // FLOW_REGISTER_MOBILE_03: RegisterAsync posts the new account data to /api/mobile/auth/register.
    // This file is involved because it converts the MAUI register action into HTTP; next step is Program.cs endpoint.
    public async Task<MobileAuthResponse?> RegisterAsync(string fullName, string email, string password) =>
        await PostJsonAsync<MobileRegisterRequest, MobileAuthResponse>(
            "api/mobile/auth/register",
            new(fullName, email, password));

    // Topic: Forgot-password API call
    // Purpose: Posts the email and mobile API base URL so the backend can send a reset email.
    // FLOW_PASSWORD_RESET_MOBILE_03: ForgotPasswordAsync asks the backend to send the reset email.
    // This file is involved because MAUI cannot send reset email itself; next step is Program.cs mobile forgot-password endpoint.
    public async Task<MobileBasicResponse?> ForgotPasswordAsync(string email) =>
        await PostJsonAsync<MobileForgotPasswordRequest, MobileBasicResponse>(
            "api/mobile/auth/forgot-password",
            new(email, BaseUri.ToString()));

    // Topic: Dashboard API call
    // Purpose: Requests home/admin dashboard count data from the backend.
    // FLOW_DASHBOARD_COUNTS_MOBILE_03: GetDashboardAsync calls /api/mobile/dashboard for mobile home/admin totals.
    // This file is involved because MAUI gets dashboard data through HTTP; next step is Program.cs dashboard endpoint.
    public async Task<MobileDashboardResponse?> GetDashboardAsync(string actorUserId)
    {
        var url = $"api/mobile/dashboard?actorUserId={Uri.EscapeDataString(actorUserId)}";
        return await GetJsonAsync<MobileDashboardResponse>(url);
    }

    // Topic: Closet API call
    // Purpose: Requests garment rows for the current user or an admin-selected customer.
    // FLOW_CLOSET_VIEW_MOBILE_03: GetClosetAsync requests garment rows from /api/mobile/closet/items.
    // This file is involved because MAUI receives JSON, not direct DB objects; next step is Program.cs access check.
    public async Task<List<Garment>> GetClosetAsync(string actorUserId, string userId)
    {
        var url = $"api/mobile/closet/items?actorUserId={Uri.EscapeDataString(actorUserId)}&userId={Uri.EscapeDataString(userId)}";
        return await GetJsonAsync<List<Garment>>(url) ?? new();
    }

    // Topic: Saved outfits API call
    // Purpose: Requests saved outfit rows for the current user or an admin-selected customer.
    // FLOW_OUTFIT_VIEW_MOBILE_03: GetOutfitsAsync requests saved outfit rows from /api/mobile/outfits/items.
    // This file is involved because MAUI receives outfit JSON; next step is Program.cs access check.
    public async Task<List<Outfit>> GetOutfitsAsync(string actorUserId, string userId)
    {
        var url = $"api/mobile/outfits/items?actorUserId={Uri.EscapeDataString(actorUserId)}&userId={Uri.EscapeDataString(userId)}";
        return await GetJsonAsync<List<Outfit>>(url) ?? new();
    }

    public async Task<List<OutfitGarment>> GetOutfitLinksAsync(IEnumerable<string> outfitIds)
    {
        var ids = outfitIds.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (ids.Count == 0)
            return new();

        // The current public outfit API returns the slot ids on each Outfit model, so this method is kept for future native detail screens.
        await Task.CompletedTask;
        return new();
    }

    // Topic: Garment upload API call
    // Purpose: Posts base64 image data and optional metadata to the backend upload endpoint.
    // FLOW_GARMENT_UPLOAD_MOBILE_03: CreateGarmentAsync posts base64 image data to /api/mobile/garments.
    // This file is involved because image analysis and DB insert happen on the server; next step is Program.cs upload endpoint.
    public async Task<MobileGarmentCreateResponse?> CreateGarmentAsync(MobileGarmentCreateRequest request) =>
        await PostJsonAsync<MobileGarmentCreateRequest, MobileGarmentCreateResponse>("api/mobile/garments", request);

    // FLOW_DELETE_GARMENT_MOBILE_03: DeleteGarmentAsync calls the backend garment delete endpoint after confirmation.
    // This file is involved because MAUI deletes through API only; next step is Program.cs ownership check.
    public async Task<bool> DeleteGarmentAsync(string userId, string garmentId)
    {
        var url = $"api/mobile/garments/{Uri.EscapeDataString(garmentId)}?userId={Uri.EscapeDataString(userId)}";
        return await DeleteAsync(url);
    }

    // Topic: Match run API call
    // Purpose: Sends seed garments and request text to the backend matcher endpoint.
    // FLOW_MATCH_RUN_MOBILE_03: RunMatchAsync posts match request data to /api/mobile/matches/run.
    // This file is involved because MatchingService lives on the backend; next step is Program.cs match endpoint.
    public async Task<MatchResult?> RunMatchAsync(MobileMatchRequest request) =>
        await PostJsonAsync<MobileMatchRequest, MatchResult>("api/mobile/matches/run", request);

    // Topic: Save recommendation API call
    // Purpose: Sends the selected recommendation to the backend so it becomes a saved outfit.
    // FLOW_MATCH_SAVE_MOBILE_02: SaveMatchAsync posts the selected recommendation to /api/mobile/matches/save.
    // This file is involved because saved outfits are created on the backend; next step is Program.cs save endpoint.
    public async Task<SaveOutfitResult?> SaveMatchAsync(MobileSaveMatchRequest request) =>
        await PostJsonAsync<MobileSaveMatchRequest, SaveOutfitResult>("api/mobile/matches/save", request);

    // FLOW_DELETE_OUTFIT_MOBILE_03: DeleteOutfitAsync calls the backend saved outfit delete endpoint after confirmation.
    // This file is involved because MAUI deletes through API only; next step is Program.cs ownership check.
    public async Task<bool> DeleteOutfitAsync(string userId, string outfitId)
    {
        var url = $"api/mobile/outfits/{Uri.EscapeDataString(outfitId)}?userId={Uri.EscapeDataString(userId)}";
        return await DeleteAsync(url);
    }

    // Topic: Admin users API call
    // Purpose: Requests the admin user list from the backend.
    // FLOW_ADMIN_USER_MANAGE_MOBILE_03: GetUsersAsync requests the admin user list from /api/mobile/users.
    // This file is involved because admin list data comes from the backend; next step is Program.cs admin check.
    public async Task<List<MobileUserDto>> GetUsersAsync(string actorUserId)
    {
        var url = $"api/mobile/users?actorUserId={Uri.EscapeDataString(actorUserId)}";
        return await GetJsonAsync<List<MobileUserDto>>(url) ?? new();
    }

    // FLOW_ADMIN_USER_MANAGE_MOBILE_03A: CreateUserAsync posts the admin create-user form to /api/mobile/users.
    // This file is involved because user creation is a backend DB action; next step is Program.cs admin create endpoint.
    public async Task<MobileUserDto?> CreateUserAsync(MobileUserCreateRequest request) =>
        await PostJsonAsync<MobileUserCreateRequest, MobileUserDto>("api/mobile/users", request);

    public async Task<MobileUserDto?> UpdateUserAsync(string userId, MobileUserUpdateRequest request)
    {
        using var response = await _http.PutAsJsonAsync(
            $"api/mobile/users/{Uri.EscapeDataString(userId)}",
            request,
            JsonOptions);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<MobileUserDto>(JsonOptions);
    }

    public async Task<bool> DeleteUserAsync(string actorUserId, string userId)
    {
        var url = $"api/mobile/users/{Uri.EscapeDataString(userId)}?actorUserId={Uri.EscapeDataString(actorUserId)}";
        return await DeleteAsync(url);
    }

    // FLOW_DELETE_ACCOUNT_MOBILE_03: DeleteAccountAsync calls /api/mobile/account after confirmation.
    // This file is involved because cascade deletion is server-side; next step is Program.cs DeleteUserCascadeAsync.
    public async Task<bool> DeleteAccountAsync(string userId)
    {
        var url = $"api/mobile/account?userId={Uri.EscapeDataString(userId)}";
        return await DeleteAsync(url);
    }

    private async Task<TResponse?> GetJsonAsync<TResponse>(string url)
    {
        using var response = await _http.GetAsync(url);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
    }

    private async Task<TResponse?> PostJsonAsync<TRequest, TResponse>(string url, TRequest request)
    {
        using var response = await _http.PostAsJsonAsync(url, request, JsonOptions);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
    }

    private async Task<bool> DeleteAsync(string url)
    {
        using var response = await _http.DeleteAsync(url);
        if (response.StatusCode == HttpStatusCode.NoContent)
            return true;

        await EnsureSuccessAsync(response);
        return response.IsSuccessStatusCode;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var message = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(message))
            message = $"{(int)response.StatusCode} {response.ReasonPhrase}";

        throw new InvalidOperationException(message.Trim('"'));
    }

    private static Uri NormalizeBaseUri(Uri uri)
    {
        var text = uri.ToString();
        if (!text.EndsWith("/", StringComparison.Ordinal))
            text += "/";
        return new Uri(text);
    }
}

public record MobileAuthRequest(string Email, string Password);

public record MobileRegisterRequest(string FullName, string Email, string Password);

public record MobileAuthResponse(bool Success, string Message, MobileUserDto? User);

public record MobileForgotPasswordRequest(string Email, string? OriginBaseUrl);

public record MobileBasicResponse(bool Success, string Message);

public record MobileDashboardResponse(
    bool IsAdmin,
    int CustomerCount,
    int AdminCount,
    int GarmentCount,
    int OutfitCount,
    IReadOnlyList<MobileUsagePoint> Usage);

public record MobileUsagePoint(DateTime Day, int UsersCreated, int GarmentsAdded, int OutfitsSaved);

public record MobileUserDto(string UserId, string Email, string FullName, string Role, DateTime CreatedAt);

public record MobileUserCreateRequest(string ActorUserId, string FullName, string Email, string Password, string Role);

public record MobileUserUpdateRequest(string ActorUserId, string FullName, string Email, string Role);

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

public record MobileGarmentCreateResponse(bool Success, string Message, bool RequiresManualType, Garment? Garment);

public record MobileMatchRequest(string UserId, string[]? SeedGarmentIds, bool AllowNoSeed, string? UserRequest);

public record MobileSaveMatchRequest(string UserId, ScoredCombination Recommendation, string[]? SeedGarmentIds);

public record MatchResult
{
    public bool Success { get; init; }
    public bool Blocked { get; init; }
    public bool NoEligible { get; init; }
    public string? NoEligibleMessage { get; init; }
    public string? ErrorMessage { get; init; }
    public int MinPerTypeRequired { get; init; }
    public Dictionary<string, int> Counts { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public List<ScoredCombination> Results { get; init; } = new();
}

public record SaveOutfitResult
{
    public bool Success { get; init; }
    public bool Duplicate { get; init; }
    public string? OutfitId { get; init; }
    public string Message { get; init; } = string.Empty;
}

public record ScoredCombination
{
    public string ShirtId { get; init; } = string.Empty;
    public string PantsId { get; init; } = string.Empty;
    public string ShoesId { get; init; } = string.Empty;
    public int Score { get; init; }
    public int Rank { get; init; }
    public string StyleLabel { get; init; } = string.Empty;
    public string Explanation { get; init; } = string.Empty;
    public string RecommendedPlaces { get; init; } = string.Empty;
}
