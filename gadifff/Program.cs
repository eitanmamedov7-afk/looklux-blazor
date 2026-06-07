
// SEARCH INDEX
// API, MOBILE, LOGIN, REGISTER, PASSWORD, RESET, COOKIE, DATABASE, GARMENT, IMAGE,
// UPLOAD, MATCH, RECOMMENDATION, OUTFIT, ADMIN, DELETE, COUNT, FILTER, VALIDATE, CONFIG
//
// Topic: APPLICATION STARTUP AND API ROUTES
// Purpose: Registers services, configures web cookies, serves Blazor pages, and exposes API endpoints for MAUI/mobile.
// Search keywords: API MOBILE LOGIN COOKIE DATABASE GARMENT MATCH OUTFIT ADMIN DELETE CONFIG
// When to use it: Open this file when explaining how the web app starts or how MAUI talks to the backend.
// Important notes: Razor pages call services directly; the MAUI app calls the MapGet/MapPost/MapDelete API endpoints here.

using gadifff.Components;
using DBL;
using gadifff.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Models;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.Json;
using BCryptNet = BCrypt.Net.BCrypt;

// SECTION: CONFIG STARTUP DATABASE API
// Topic: Startup builder
// Purpose: Creates the ASP.NET Core app builder and loads configuration sources.
// Search keywords: CONFIG STARTUP API
// When to use it: Show this when explaining where the app begins.
// Important notes: secrets.json, user-secrets, and environment variables can supply API keys/SMTP values.
var builder = WebApplication.CreateBuilder(args);

// Configuration sources:
// secrets.json/user-secrets/environment variables supply API keys, SMTP settings, and prompts.
builder.Configuration
    .AddJsonFile("secrets.json", optional: true, reloadOnChange: true);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
}

builder.Configuration.AddEnvironmentVariables();

// Blazor Server UI setup for all pages under Components.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddDataProtection();

builder.Services.AddHttpContextAccessor();

// Topic: Database dependency registration
// Purpose: Makes DB classes available to pages, services, and API endpoints through dependency injection.
// Search keywords: DATABASE DBL ADD LIST COUNT SEARCH UPDATE REMOVE
// When to use it: Show this when explaining how code gets access to MySQL tables.
// Important notes: These registrations do not run SQL; they only let ASP.NET create the DB helper objects.
builder.Services.AddScoped<UserDB>();
builder.Services.AddScoped<GarmentDB>();
builder.Services.AddScoped<OutfitDB>();
builder.Services.AddScoped<OutfitGarmentDB>();
builder.Services.AddScoped<OutfitWearLogDB>();

// Topic: Authentication support services
// Purpose: Registers email sender and AuthService for login, register, forgot password, and reset password.
// Search keywords: LOGIN REGISTER PASSWORD RESET EMAIL VALIDATE COOKIE
// When to use it: Show this when explaining auth flow ownership.
// Important notes: Web uses cookies; MAUI/mobile receives API response data.
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.PostConfigure<SmtpOptions>(options =>
    ApplyLegacySmtpOverrides(options, builder.Configuration));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddScoped<IAuthService, AuthService>();

// Topic: AI garment analysis and matching services
// Purpose: Registers image analysis and recommendation logic used by both web and API flows.
// Search keywords: GARMENT IMAGE UPLOAD MATCH RECOMMENDATION API
// When to use it: Show this when explaining AI feature extraction or outfit suggestions.
// Important notes: GeminiClient is shared by upload analysis and outfit scoring.
builder.Services.AddScoped<GarmentFeatureService>();
builder.Services.AddScoped<MatchingService>();
builder.Services.AddHttpClient<GeminiClient>();

// Topic: Web cookie authentication
// Purpose: Configures browser login sessions for Blazor pages.
// Search keywords: COOKIE LOGIN LOGOUT AUTH VALIDATE
// When to use it: Show this when explaining how the web remembers a logged-in user.
// Important notes: Cookie middleware is registered here, but current page checks read the active user through AuthService.CurrentUserAsync.
// FLOW_AUTH_STATE_WEB_00: Program.cs registers cookie auth middleware and puts authentication into the ASP.NET request pipeline.
// This file is involved because middleware is configured at startup; next step is AuthService storing/returning the project user state.
// FLOW_AUTH_STATE_MOBILE_00: MAUI does not use this cookie middleware; it uses API responses and MainPage._currentUser.
// This file is involved because mobile API endpoints return DTOs instead of creating a browser cookie.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/accessdenied";
    });

// Swagger documents the internal/mobile API during development.
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "gadifff Internal API",
        Version = "v1",
        Description = "Internal endpoints used by the MAUI/mobile app and web frontend."
    });
});

var app = builder.Build();
var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
// Startup checks make prompt/API-key problems visible before users try upload or matching.
LogGarmentPromptConfiguration(app, startupLogger);
LogGeminiConfiguration(app, startupLogger);

// Production safety middleware.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

// Request pipeline order:
// routing -> auth -> authorization -> Blazor/API endpoints.
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "gadifff Internal API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseAntiforgery();

// Media endpoint:
// closet/outfit cards call this to render the image stored directly on garments.image_bytes.
// SECTION: IMAGE API GARMENT
// FLOW_GARMENT_IMAGE_SERVE_02: Image URL reaches Program.cs; it asks GarmentDB for bytes.
// This file is involved because browsers/MAUI need a URL endpoint; next step is GarmentDB.GetImageByGarmentIdAsync.
// Topic: Garment image endpoint
// Purpose: Returns stored image bytes for one garment so web/MAUI can display photos.
// Search keywords: IMAGE API GARMENT DATABASE SEARCH
// When to use it: Show this when explaining why images use a URL instead of embedding bytes in pages.
// Important notes: Reads from the merged garments table, not from a separate garment_images table.
app.MapGet("/media/garments/by-garment/{garmentId}", async (string garmentId, GarmentDB garmentDb) =>
{
    var img = await garmentDb.GetImageByGarmentIdAsync(garmentId);
    if (img == null) return Results.NotFound();

    return Results.File(img.Value.Bytes, img.Value.MimeType);
})
.WithName("GetGarmentImageByGarmentId")
.WithTags("Media");

// Internal web/mobile closet API: returns filtered garment records without image bytes.
// SECTION: WEB API CLOSET FILTER
// FLOW_CLOSET_FILTER_API_02: API closet item request reaches Program.cs with query-string filters.
// This file is involved because API calls need query parsing; next step is ParseGarmentFilterRequest and GarmentDB.
// Topic: Web closet data endpoints
// Purpose: Supplies garment list/filter data to browser-side closet features.
// Search keywords: API CLOSET GARMENT LIST FILTER SEARCH DATABASE
// When to use it: Show this when explaining how closet data can be queried from the backend.
// Important notes: The Razor page also uses GarmentDB directly; these endpoints are for API-style access.
app.MapGet("/api/closet/items", async (HttpRequest request, GarmentDB garmentDb) =>
{
    var userId = request.Query["userId"].ToString();
    if (string.IsNullOrWhiteSpace(userId))
        return Results.BadRequest("userId is required.");

    var filter = ParseGarmentFilterRequest(request.Query);
    var items = await garmentDb.GetFilteredByUserAsync(userId, filter, 1000);
    return Results.Ok(items);
})
.WithName("GetClosetItems")
.WithTags("Internal API");

// Internal closet filter API: returns available filter choices for the current filter state.
app.MapGet("/api/closet/filters", async (HttpRequest request, GarmentDB garmentDb) =>
{
    var userId = request.Query["userId"].ToString();
    if (string.IsNullOrWhiteSpace(userId))
        return Results.BadRequest("userId is required.");

    var filter = ParseGarmentFilterRequest(request.Query);
    var options = await garmentDb.GetFilterOptionsAsync(userId, filter);
    return Results.Ok(options);
})
.WithName("GetClosetFilters")
.WithTags("Internal API");

// Internal outfit API: returns saved outfits after applying score/style/place filters.
app.MapGet("/api/outfits/items", async (HttpRequest request, OutfitDB outfitDb) =>
{
    var userId = request.Query["userId"].ToString();
    if (string.IsNullOrWhiteSpace(userId))
        return Results.BadRequest("userId is required.");

    var filter = ParseOutfitFilterRequest(request.Query);
    var outfits = await outfitDb.GetFilteredByUserAsync(userId, filter, 400);
    return Results.Ok(outfits);
})
.WithName("GetOutfitItems")
.WithTags("Internal API");

// Internal outfit filter API: returns available outfit filter choices.
app.MapGet("/api/outfits/filters", async (HttpRequest request, OutfitDB outfitDb) =>
{
    var userId = request.Query["userId"].ToString();
    if (string.IsNullOrWhiteSpace(userId))
        return Results.BadRequest("userId is required.");

    var filter = ParseOutfitFilterRequest(request.Query);
    var options = await outfitDb.GetFilterOptionsAsync(userId, filter);
    return Results.Ok(options);
})
.WithName("GetOutfitFilters")
.WithTags("Internal API");

// Mobile auth login:
// MAUI posts email/password and receives the same user identity fields the web session uses.
// SECTION: MOBILE API AUTH LOGIN
// FLOW_LOGIN_MOBILE_04: Program.cs receives /api/mobile/auth/login and validates credentials.
// This file is involved because it is the mobile API boundary; next step is UserDB.GetByEmailAsync and BCrypt check.
// FLOW_AUTH_STATE_MOBILE_03: Program.cs returns MobileAuthResponse.User after successful BCrypt validation.
// This file is involved because MAUI stores this returned DTO in MainPage._currentUser instead of using a cookie.
// Topic: Mobile login endpoint
// Purpose: Validates MAUI login credentials and returns a safe user DTO.
// Search keywords: API MOBILE LOGIN VALIDATE USER PASSWORD
// When to use it: Show this when explaining how MAUI signs in through the backend.
// Important notes: It checks BCrypt password hash but does not create a browser cookie.
app.MapPost("/api/mobile/auth/login", async (MobileAuthRequest request, UserDB userDb) =>
{
    var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
    // VALIDATION_EMAIL / VALIDATION_PASSWORD: mobile login requires a valid email shape and non-empty password before DB lookup.
    if (!IsValidEmail(email) || string.IsNullOrWhiteSpace(request.Password))
        return Results.Ok(new MobileAuthResponse(false, "Invalid email or password.", null));

    var user = (await userDb.GetByEmailAsync(email)).FirstOrDefault();
    if (user == null ||
        string.IsNullOrWhiteSpace(user.PasswordHash) ||
        !BCryptNet.Verify(request.Password ?? string.Empty, user.PasswordHash))
    {
        return Results.Ok(new MobileAuthResponse(false, "Invalid email or password.", null));
    }

    return Results.Ok(new MobileAuthResponse(true, "Signed in.", MobileUserDto.FromUser(user)));
})
.WithName("MobileLogin")
.WithTags("Mobile API");

// Mobile registration creates customer accounts only, matching the public web registration flow.
// Topic: Mobile register endpoint
// FLOW_REGISTER_MOBILE_04: Program.cs receives /api/mobile/auth/register, validates fields, hashes password.
// This file is involved because account creation is server-side; next step is UserDB.CreateAsync.
// Purpose: Creates a new customer account from the MAUI app.
// Search keywords: API MOBILE REGISTER ADD USER VALIDATE DATABASE
// When to use it: Show this when explaining mobile account creation.
// Important notes: Password is hashed before insert; role is customer by default.
app.MapPost("/api/mobile/auth/register", async (MobileRegisterRequest request, UserDB userDb) =>
{
    var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
    var password = request.Password ?? string.Empty;

    // VALIDATION_NAME / VALIDATION_EMAIL / VALIDATION_PASSWORD: mobile registration validates every user-entered identity field.
    if (!FullNameRules.TryNormalize(request.FullName, out var fullName))
        return Results.BadRequest(new MobileAuthResponse(false, FullNameRules.ValidationMessage, null));

    if (!IsValidEmail(email) || string.IsNullOrWhiteSpace(password))
        return Results.BadRequest(new MobileAuthResponse(false, "Email and password are required.", null));

    // VALIDATION_PASSWORD: mobile registration enforces project password length before hashing.
    if (!IsValidPassword(password))
        return Results.BadRequest(new MobileAuthResponse(false, "Password must be 6-128 characters.", null));

    if ((await userDb.GetByEmailAsync(email)).Any())
        return Results.Conflict(new MobileAuthResponse(false, "Email already exists.", null));

    var user = new User
    {
        UserId = Guid.NewGuid().ToString(),
        Email = email,
        FullName = fullName,
        Role = "customer",
        PasswordHash = BCryptNet.HashPassword(password),
        CreatedAt = DateTime.UtcNow
    };

    var inserted = await userDb.CreateAsync(user);
    if (inserted <= 0)
        return Results.Problem("Could not create account.");

    return Results.Ok(new MobileAuthResponse(true, "Account created.", MobileUserDto.FromUser(user)));
})
.WithName("MobileRegister")
.WithTags("Mobile API");

// Mobile forgot-password reuses AuthService so email tokens behave the same as the web page.
// Topic: Mobile forgot-password endpoint
// Purpose: Lets MAUI start the same password reset email flow as the web.
// Search keywords: API MOBILE PASSWORD RESET EMAIL VALIDATE
// When to use it: Show this when explaining that web and MAUI share forgot-password logic.
// Important notes: Delegates token/email creation to AuthService.
// FLOW_PASSWORD_RESET_MOBILE_04: Program.cs receives /api/mobile/auth/forgot-password and resolves a browser-openable reset origin.
// This file is involved because MAUI uses emulator API URLs like 10.0.2.2; next step is UserDB account lookup inside AuthService.
app.MapPost("/api/mobile/auth/forgot-password", async (
    MobileForgotPasswordRequest request,
    HttpRequest httpRequest,
    IConfiguration configuration,
    IAuthService authService) =>
{
    var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
    var origin = ResolvePasswordResetOrigin(request.OriginBaseUrl, httpRequest, configuration);

    // VALIDATION_EMAIL: forgot-password only accepts a real email shape before AuthService creates a reset email.
    if (!IsValidEmail(email))
        return Results.Ok(new MobileBasicResponse(false, "Enter a valid email address."));

    var status = await authService.SendPasswordResetEmailAsync(email, origin);
    var response = status switch
    {
        PasswordResetEmailStatus.Sent => new MobileBasicResponse(true, "A password reset link has been sent."),
        PasswordResetEmailStatus.EmailNotFound => new MobileBasicResponse(false, "No account was found with this email."),
        PasswordResetEmailStatus.SendFailed => new MobileBasicResponse(false, "Could not send reset email. Check SMTP/Resend settings."),
        _ => new MobileBasicResponse(false, "Invalid reset request. Please try again.")
    };

    return Results.Ok(response);
})
.WithName("MobileForgotPassword")
.WithTags("Mobile API");

// Mobile reset-password completes the token flow started by forgot-password.
app.MapPost("/api/mobile/auth/reset-password", async (
    MobileResetPasswordRequest request,
    IAuthService authService) =>
{
    // VALIDATION_PASSWORD: reset-password requires the same password length rule as registration.
    if (!IsValidPassword(request.NewPassword))
        return Results.Ok(new MobileBasicResponse(false, "Password must be 6-128 characters."));

    var ok = await authService.ResetPasswordAsync(request.Token ?? string.Empty, request.NewPassword ?? string.Empty);
    return Results.Ok(new MobileBasicResponse(
        ok,
        ok ? "Password was reset." : "Reset link is invalid or expired."));
})
.WithName("MobileResetPassword")
.WithTags("Mobile API");

// Mobile dashboard:
// admins receive global counts/chart points, customers receive their own closet/outfit counts.
// SECTION: MOBILE API DASHBOARD COUNT
// FLOW_DASHBOARD_COUNTS_MOBILE_04: Program.cs receives dashboard request and chooses admin or customer count data.
// This file is involved because the dashboard aggregates multiple DB classes; next step is UserDB/GarmentDB/OutfitDB counts.
// Topic: Mobile dashboard counts
// Purpose: Returns counts/charts for home/admin dashboard data in MAUI.
// Search keywords: API MOBILE DASHBOARD COUNT ADMIN LIST
// When to use it: Show this when explaining charts or summary totals.
// Important notes: Admin receives global counts; customer receives own garment/outfit counts.
app.MapGet("/api/mobile/dashboard", async (
    string actorUserId,
    UserDB userDb,
    GarmentDB garmentDb,
    OutfitDB outfitDb) =>
{
    // VALIDATION_USER_ID: dashboard requests must identify the acting user before any counts are loaded.
    if (!IsValidGuidText(actorUserId))
        return Results.BadRequest("actorUserId is required.");

    var actor = await userDb.GetByIdAsync(actorUserId);
    if (actor == null)
        return Results.BadRequest("actorUserId is required.");

    if (IsAdmin(actor))
    {
        const int chartDays = 14;
        var usersByDay = await userDb.GetDailyCreatedCountsAsync(chartDays);
        var garmentsByDay = await garmentDb.GetDailyCreatedCountsByUserRoleAsync(chartDays, "customer");
        var outfitsByDay = await outfitDb.GetDailyCreatedCountsByUserRoleAsync(chartDays, "customer");

        return Results.Ok(new MobileDashboardResponse(
            true,
            await userDb.CountByRoleAsync("customer"),
            await userDb.CountByRoleAsync("admin"),
            await garmentDb.CountByUserRoleAsync("customer"),
            await outfitDb.CountByUserRoleAsync("customer"),
            BuildMobileUsagePoints(chartDays, usersByDay, garmentsByDay, outfitsByDay)));
    }

    var garments = await garmentDb.GetByUserAsync(actor.UserId);
    var outfits = await outfitDb.GetByUserAsync(actor.UserId, 400);
    return Results.Ok(new MobileDashboardResponse(
        false,
        0,
        0,
        garments.Count,
        outfits.Count,
        Array.Empty<MobileUsagePoint>()));
})
.WithName("MobileDashboard")
.WithTags("Mobile API");

// Mobile closet list:
// customers see themselves; admins can pass userId to inspect a customer's closet.
// SECTION: MOBILE API CLOSET OUTFITS
// FLOW_CLOSET_VIEW_MOBILE_04: Program.cs receives /api/mobile/closet/items, checks actor access, and loads garments.
// This file is involved because admins may view another user; next step is GarmentDB.GetByUserAsync.
// Topic: Mobile closet list endpoint
// Purpose: Returns garments for the signed-in user or admin-selected customer.
// Search keywords: API MOBILE CLOSET GARMENT LIST ADMIN SEARCH
// When to use it: Show this when explaining how MAUI displays closet data.
// Important notes: Admin may view another user; normal customer only sees own data.
app.MapGet("/api/mobile/closet/items", async (
    string actorUserId,
    string? userId,
    UserDB userDb,
    GarmentDB garmentDb) =>
{
    // VALIDATION_USER_ID: closet API validates actor and optional target user ids before authorization checks.
    if (!IsValidGuidText(actorUserId) || (!string.IsNullOrWhiteSpace(userId) && !IsValidGuidText(userId)))
        return Results.BadRequest("Invalid user id.");

    var actor = await userDb.GetByIdAsync(actorUserId);
    if (actor == null)
        return Results.BadRequest("actorUserId is required.");

    var targetUserId = string.IsNullOrWhiteSpace(userId) ? actor.UserId : userId.Trim();
    if (!CanAccessUserData(actor, targetUserId))
        return Results.Forbid();

    var items = await garmentDb.GetByUserAsync(targetUserId);
    return Results.Ok(items.OrderByDescending(x => x.CreatedAt));
})
.WithName("MobileClosetItems")
.WithTags("Mobile API");

// Mobile outfit list:
// same access rule as closet items, with admin support for customer viewing.
// Topic: Mobile outfit list endpoint
// FLOW_OUTFIT_VIEW_MOBILE_04: Program.cs receives /api/mobile/outfits/items, checks actor access, and loads outfits.
// This file is involved because admins may view another user; next step is OutfitDB.GetByUserAsync.
// FLOW_OUTFIT_WEAR_STATS_MOBILE_01: Mobile outfit list endpoint also loads wear summaries for each returned outfit.
// This file is involved because MAUI receives outfit JSON only; next step is OutfitWearLogDB summary aggregation.
// Purpose: Returns saved outfit recommendations for MAUI.
// Search keywords: API MOBILE OUTFIT LIST RECOMMENDATION ADMIN OUTFIT_WEAR_LOG COUNT TIMESTAMP
// When to use it: Show this when explaining saved outfit display in MAUI.
// Important notes: Uses OutfitDB plus OutfitWearLogDB; images are later loaded through the media endpoint.
app.MapGet("/api/mobile/outfits/items", async (
    string actorUserId,
    string? userId,
    UserDB userDb,
    OutfitDB outfitDb,
    OutfitWearLogDB outfitWearLogDb) =>
{
    // VALIDATION_USER_ID: outfit API validates actor and optional target user ids before authorization checks.
    if (!IsValidGuidText(actorUserId) || (!string.IsNullOrWhiteSpace(userId) && !IsValidGuidText(userId)))
        return Results.BadRequest("Invalid user id.");

    var actor = await userDb.GetByIdAsync(actorUserId);
    if (actor == null)
        return Results.BadRequest("actorUserId is required.");

    var targetUserId = string.IsNullOrWhiteSpace(userId) ? actor.UserId : userId.Trim();
    if (!CanAccessUserData(actor, targetUserId))
        return Results.Forbid();

    var outfits = await outfitDb.GetByUserAsync(targetUserId, 400);
    var summaries = await outfitWearLogDb.GetSummariesByOutfitIdsAsync(outfits.Select(x => x.OutfitId));
    ApplyOutfitWearSummaries(outfits, summaries);
    return Results.Ok(outfits.OrderByDescending(x => x.CreatedAt));
})
.WithName("MobileOutfitItems")
.WithTags("Mobile API");

// Mobile admin customer list for the MAUI admin dashboard.
// SECTION: MOBILE API ADMIN USERS
// FLOW_ADMIN_USER_MANAGE_MOBILE_04: Program.cs receives admin user-list request and verifies the actor is admin.
// This file is involved because user management is protected API data; next step is UserDB.GetAllAsync.
// Topic: Mobile admin user list
// Purpose: Lets an admin load customer/admin accounts in MAUI.
// Search keywords: API MOBILE ADMIN USER LIST SEARCH
// When to use it: Show this when explaining admin dashboard/customer management.
// Important notes: Returns safe DTOs and never exposes password hashes.
app.MapGet("/api/mobile/users", async (string actorUserId, UserDB userDb) =>
{
    // VALIDATION_USER_ID: admin user list requires a valid acting user id before admin-role check.
    if (!IsValidGuidText(actorUserId))
        return Results.BadRequest("Invalid actor user id.");

    var actor = await userDb.GetByIdAsync(actorUserId);
    if (!IsAdmin(actor))
        return Results.Forbid();

    var users = (await userDb.GetAllAsync())
        .OrderByDescending(x => x.CreatedAt)
        .Select(MobileUserDto.FromUser)
        .ToList();
    return Results.Ok(users);
})
.WithName("MobileGetUsers")
.WithTags("Mobile API");

// Mobile admin creates customer/admin accounts, matching AdminClosets.razor behavior.
app.MapPost("/api/mobile/users", async (MobileUserCreateRequest request, UserDB userDb) =>
{
    // VALIDATION_USER_ID: admin create-user requires a valid acting user id before admin-role check.
    if (!IsValidGuidText(request.ActorUserId))
        return Results.BadRequest("Invalid actor user id.");

    var actor = await userDb.GetByIdAsync(request.ActorUserId);
    if (!IsAdmin(actor))
        return Results.Forbid();

    var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
    var password = request.Password ?? string.Empty;
    var role = NormalizeRole(request.Role);

    // VALIDATION_NAME / VALIDATION_EMAIL / VALIDATION_PASSWORD: admin-created users validate all entered identity fields.
    if (!FullNameRules.TryNormalize(request.FullName, out var fullName))
        return Results.BadRequest(FullNameRules.ValidationMessage);

    if (!IsValidEmail(email) || string.IsNullOrWhiteSpace(password))
        return Results.BadRequest("Email and password are required.");

    // VALIDATION_PASSWORD: admin-created passwords follow the same 6-128 length rule.
    if (!IsValidPassword(password))
        return Results.BadRequest("Password must be 6-128 characters.");

    if ((await userDb.GetByEmailAsync(email)).Any())
        return Results.Conflict("Email already exists.");

    var user = new User
    {
        UserId = Guid.NewGuid().ToString(),
        FullName = fullName,
        Email = email,
        Role = role,
        PasswordHash = BCryptNet.HashPassword(password),
        CreatedAt = DateTime.UtcNow
    };

    var created = await userDb.CreateAsync(user);
    return created > 0 ? Results.Ok(MobileUserDto.FromUser(user)) : Results.Problem("Could not create user.");
})
.WithName("MobileCreateUser")
.WithTags("Mobile API");

// Mobile admin updates account identity/role while protecting the last admin.
app.MapPut("/api/mobile/users/{userId}", async (string userId, MobileUserUpdateRequest request, UserDB userDb) =>
{
    // VALIDATION_USER_ID: admin update requires valid target and actor ids before DB lookup.
    if (!IsValidGuidText(userId) || !IsValidGuidText(request.ActorUserId))
        return Results.BadRequest("Invalid user id.");

    var actor = await userDb.GetByIdAsync(request.ActorUserId);
    if (!IsAdmin(actor))
        return Results.Forbid();

    var user = await userDb.GetByIdAsync(userId);
    if (user == null)
        return Results.NotFound();

    var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
    var role = NormalizeRole(request.Role);

    // VALIDATION_NAME / VALIDATION_EMAIL: admin updates require a valid display name and email shape.
    if (!FullNameRules.TryNormalize(request.FullName, out var fullName))
        return Results.BadRequest(FullNameRules.ValidationMessage);

    if (!IsValidEmail(email))
        return Results.BadRequest("Enter a valid email address.");

    if (string.Equals(actor.UserId, user.UserId, StringComparison.OrdinalIgnoreCase) && role != "admin")
        return Results.BadRequest("You cannot remove your own admin role.");

    var duplicates = await userDb.GetByEmailAsync(email);
    if (duplicates.Any(x => !string.Equals(x.UserId, user.UserId, StringComparison.OrdinalIgnoreCase)))
        return Results.Conflict("Another account already uses that email.");

    var persistedRole = NormalizeRole(user.Role);
    if (persistedRole == "admin" && role != "admin" && await userDb.CountByRoleAsync("admin") <= 1)
        return Results.BadRequest("Cannot remove the last admin account.");

    user.Email = email;
    user.FullName = fullName;
    user.Role = role;

    var updated = await userDb.UpdateUserAsync(user);
    return updated > 0 ? Results.Ok(MobileUserDto.FromUser(user)) : Results.Problem("No changes were saved.");
})
.WithName("MobileUpdateUser")
.WithTags("Mobile API");

// Mobile admin delete:
// admins can delete whole accounts, but not individual customer garments/outfits from wardrobe views.
app.MapDelete("/api/mobile/users/{userId}", async (
    string userId,
    string actorUserId,
    UserDB userDb,
    GarmentDB garmentDb,
    OutfitDB outfitDb,
    OutfitGarmentDB outfitGarmentDb) =>
{
    // VALIDATION_USER_ID: admin delete validates actor and target ids before authorization/deletion.
    if (!IsValidGuidText(actorUserId) || !IsValidGuidText(userId))
        return Results.BadRequest("Invalid user id.");

    var actor = await userDb.GetByIdAsync(actorUserId);
    if (!IsAdmin(actor))
        return Results.Forbid();

    if (string.Equals(actor.UserId, userId, StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest("You cannot delete your own admin account.");

    var user = await userDb.GetByIdAsync(userId);
    if (user == null)
        return Results.NotFound();

    if (NormalizeRole(user.Role) == "admin" && await userDb.CountByRoleAsync("admin") <= 1)
        return Results.BadRequest("Cannot delete the last admin account.");

    var deleted = await DeleteUserCascadeAsync(user.UserId, userDb, garmentDb, outfitDb, outfitGarmentDb);
    return deleted > 0 ? Results.NoContent() : Results.Problem("Could not delete user.");
})
.WithName("MobileDeleteUser")
.WithTags("Mobile API");

// Mobile self-delete:
// the signed-in user can delete their own account after client-side confirmation.
// SECTION: MOBILE API DELETE ACCOUNT
// FLOW_DELETE_ACCOUNT_MOBILE_04: Program.cs receives /api/mobile/account and validates the user can be deleted.
// This file is involved because account deletion affects several tables; next step is DeleteUserCascadeAsync.
// Topic: Self delete account endpoint
// Purpose: Deletes the signed-in user's account and related wardrobe data.
// Search keywords: API MOBILE DELETE ACCOUNT REMOVE USER GARMENT OUTFIT DATABASE
// When to use it: Show this when explaining the delete account button in MAUI.
// Important notes: Deletes dependent outfit links/outfits/garments before deleting the user.
app.MapDelete("/api/mobile/account", async (
    string userId,
    UserDB userDb,
    GarmentDB garmentDb,
    OutfitDB outfitDb,
    OutfitGarmentDB outfitGarmentDb) =>
{
    // VALIDATION_USER_ID: self-delete validates the user id before loading the account.
    if (!IsValidGuidText(userId))
        return Results.BadRequest("Invalid user id.");

    var user = await userDb.GetByIdAsync(userId);
    if (user == null)
        return Results.NotFound();

    if (NormalizeRole(user.Role) == "admin" && await userDb.CountByRoleAsync("admin") <= 1)
        return Results.BadRequest("Cannot delete the last admin account.");

    var deleted = await DeleteUserCascadeAsync(user.UserId, userDb, garmentDb, outfitDb, outfitGarmentDb);
    return deleted > 0 ? Results.NoContent() : Results.Problem("Could not delete account.");
})
.WithName("MobileDeleteAccount")
.WithTags("Mobile API");

// Mobile garment upload:
// accepts base64 image, optionally analyzes it with Gemini, and stores the merged garment/image row.
// SECTION: MOBILE API UPLOAD GARMENT
// FLOW_GARMENT_UPLOAD_MOBILE_04: Program.cs receives base64 image data and starts validation/AI analysis.
// This file is involved because upload processing is server-side; next step is GarmentFeatureService and GarmentDB.CreateAsync.
// Topic: Mobile garment upload endpoint
// Purpose: Receives image bytes/base64 from MAUI, analyzes features, and saves a garment.
// Search keywords: API MOBILE UPLOAD GARMENT IMAGE ADD AI VALIDATE DATABASE
// When to use it: Show this when explaining the complete upload pipeline.
// Important notes: Image bytes are stored on the garment row; duplicate image SHA is rejected by DB logic.
app.MapPost("/api/mobile/garments", async (
    MobileGarmentCreateRequest request,
    GarmentDB garmentDb,
    GarmentFeatureService featureService) =>
{
    // VALIDATION_USER_ID: upload must be tied to a valid user id.
    if (!IsValidGuidText(request.UserId))
        return Results.BadRequest(new MobileGarmentCreateResponse(false, "Missing user id.", false, null));

    byte[] imageBytes;
    try
    {
        // VALIDATION_IMAGE_BASE64: image bytes must be valid base64 before AI analysis or DB insert.
        imageBytes = Convert.FromBase64String(request.ImageBase64 ?? string.Empty);
    }
    catch
    {
        return Results.BadRequest(new MobileGarmentCreateResponse(false, "Invalid image data.", false, null));
    }

    // VALIDATION_IMAGE_REQUIRED: upload must include non-empty image bytes.
    if (imageBytes.Length == 0)
        return Results.BadRequest(new MobileGarmentCreateResponse(false, "Image is required.", false, null));

    // VALIDATION_IMAGE_SIZE: upload is capped at 6MB before memory/AI/database work.
    if (imageBytes.Length > 6 * 1024 * 1024)
        return Results.BadRequest(new MobileGarmentCreateResponse(false, "Image too large (max 6MB).", false, null));

    var mimeType = string.IsNullOrWhiteSpace(request.MimeType) ? "image/jpeg" : request.MimeType.Trim();
    var fileName = string.IsNullOrWhiteSpace(request.FileName) ? "upload.jpg" : request.FileName.Trim();
    // VALIDATION_IMAGE_MIME / VALIDATION_IMAGE_FILENAME: upload metadata must be image-only and not path-like.
    if (!IsValidImageMimeType(mimeType) || !IsValidFileName(fileName))
        return Results.BadRequest(new MobileGarmentCreateResponse(false, "Invalid image file.", false, null));
    var featureJson = "{}";
    var parsed = MobileGarmentFeatures.Empty;

    if (request.AnalyzeWithAi)
    {
        try
        {
            featureJson = await featureService.ExtractFromImageAsync(imageBytes, fileName, mimeType);
            parsed = ParseMobileFeatures(featureJson);
        }
        catch (Exception ex)
        {
            if (string.IsNullOrWhiteSpace(request.Type))
            {
                return Results.Ok(new MobileGarmentCreateResponse(
                    false,
                    $"{ex.Message} Choose the type manually.",
                    true,
                    null));
            }
        }
    }

    var type = NormalizeGarmentType(FirstOptional(parsed.Type, request.Type));
    if (string.IsNullOrWhiteSpace(type))
    {
        return Results.Ok(new MobileGarmentCreateResponse(
            false,
            "Choose whether this image is a shirt, pants, or shoes.",
            true,
            null));
    }

    var garment = new Garment
    {
        GarmentId = Guid.NewGuid().ToString(),
        OwnerUserId = request.UserId.Trim(),
        Type = type,
        Color = FirstOptional(parsed.Color, request.Color),
        ColorSecondary = FirstOptional(parsed.ColorSecondary, request.ColorSecondary),
        Pattern = FirstOptional(parsed.Pattern, request.Pattern),
        StyleCategory = FirstOptional(parsed.StyleCategory, request.StyleCategory),
        Season = FirstOptional(parsed.Season, request.Season),
        Occasion = FirstOptional(parsed.Occasion, request.Occasion),
        FormalityLevel = parsed.FormalityLevel ?? request.FormalityLevel,
        StyleTags = FirstOptional(parsed.StyleTags, request.StyleTags),
        Fit = FirstOptional(parsed.Fit, request.Fit),
        Material = FirstOptional(parsed.Material, request.Material),
        Sleeve = FirstOptional(parsed.Sleeve, request.Sleeve, type == "shirt" ? null : "none"),
        Length = FirstOptional(parsed.Length, request.Length),
        Brand = FirstOptional(parsed.Brand, request.Brand),
        FeatureJson = featureJson,
        ImageBytes = imageBytes,
        ImageMimeType = mimeType,
        Sha256 = Sha256Hex(imageBytes),
        CreatedAt = DateTime.UtcNow
    };

    var inserted = await garmentDb.CreateAsync(garment);
    if (inserted <= 0)
    {
        var message = string.IsNullOrWhiteSpace(garmentDb.LastError)
            ? "Failed to add garment."
            : $"Failed to add garment: {garmentDb.LastError}";
        return Results.Problem(message);
    }

    garment.ImageBytes = null;
    return Results.Ok(new MobileGarmentCreateResponse(true, "Added.", false, garment));
})
.WithName("MobileCreateGarment")
.WithTags("Mobile API");

// Mobile garment delete for customer-owned garments.
// FLOW_DELETE_GARMENT_MOBILE_04: Program.cs checks garment ownership before deleting through GarmentDB.
// This file is involved because MAUI delete requests must be authorized server-side; next step is GarmentDB.DeleteGarmentAsync.
app.MapDelete("/api/mobile/garments/{garmentId}", async (
    string garmentId,
    string userId,
    GarmentDB garmentDb) =>
{
    // VALIDATION_GARMENT_ID / VALIDATION_USER_ID: garment delete validates route/query ids before ownership check.
    if (!IsValidGuidText(garmentId) || !IsValidGuidText(userId))
        return Results.BadRequest("Invalid garment or user id.");

    var garment = await garmentDb.GetByIdAsync(garmentId);
    if (garment == null)
        return Results.NotFound();

    if (!string.Equals(garment.OwnerUserId, userId, StringComparison.OrdinalIgnoreCase))
        return Results.Forbid();

    var deleted = await garmentDb.DeleteGarmentAsync(garmentId);
    return deleted > 0 ? Results.NoContent() : Results.Problem("Could not delete garment.");
})
.WithName("MobileDeleteGarment")
.WithTags("Mobile API");

// Mobile matcher run:
// uses the same MatchingService as the Blazor slot matcher.
// SECTION: MOBILE API MATCH RECOMMENDATION
// Topic: Mobile run match endpoint
// Purpose: Runs the same recommendation engine used by the Blazor matcher.
// Search keywords: API MOBILE MATCH RECOMMENDATION LIST VALIDATE
// When to use it: Show this when explaining that MAUI and web share MatchingService.
// Important notes: Requires at least the project minimum garments per type inside MatchingService.
// FLOW_MATCH_RUN_MOBILE_04: Program.cs receives /api/mobile/matches/run and calls MatchingService.FindMatchesAsync.
// This file is involved because the API endpoint bridges MAUI to the shared recommendation service; next step is MatchingService validation/scoring.
app.MapPost("/api/mobile/matches/run", async (MobileMatchRequest request, MatchingService matching) =>
{
    // VALIDATION_USER_ID / VALIDATION_MATCH_PROMPT: recommendation requests require a valid user and bounded prompt text.
    if (!IsValidGuidText(request.UserId) || !IsValidPrompt(request.UserRequest))
        return Results.BadRequest(MatchResult.CreateError("Invalid match request."));

    // VALIDATION_GARMENT_ID: optional seed garment ids must be GUID-shaped before matching.
    if ((request.SeedGarmentIds ?? Array.Empty<string>()).Any(id => !IsValidGuidText(id)))
        return Results.BadRequest(MatchResult.CreateError("Invalid garment selection."));

    var result = await matching.FindMatchesAsync(
        request.UserId,
        request.SeedGarmentIds ?? Array.Empty<string>(),
        cachedGarments: null,
        allowNoSeed: request.AllowNoSeed,
        userRequest: request.UserRequest);
    return Results.Ok(result);
})
.WithName("MobileRunMatch")
.WithTags("Mobile API");

// Mobile matcher save:
// persists a selected recommendation as outfit + outfit_garments links.
// Topic: Mobile save match endpoint
// FLOW_MATCH_SAVE_MOBILE_03: Program.cs receives /api/mobile/matches/save and calls MatchingService.SaveOutfitSuggestionAsync.
// This file is involved because saving recommendations must happen in backend DB classes.
// Purpose: Saves one recommendation as an outfit plus outfit_garment links.
// Search keywords: API MOBILE MATCH SAVE ADD OUTFIT DATABASE
// When to use it: Show this when explaining how recommended outfits become saved outfits.
// Important notes: Duplicate combinations are blocked by MatchingService/OutfitDB.
app.MapPost("/api/mobile/matches/save", async (MobileSaveMatchRequest request, MatchingService matching) =>
{
    // VALIDATION_USER_ID: saving a recommendation requires a valid outfit owner user id.
    if (!IsValidGuidText(request.UserId))
        return Results.BadRequest(SaveOutfitResult.Fail("Invalid user id."));

    // VALIDATION_GARMENT_ID: optional seed garment ids must be GUID-shaped before saving outfit links.
    if ((request.SeedGarmentIds ?? Array.Empty<string>()).Any(id => !IsValidGuidText(id)))
        return Results.BadRequest(SaveOutfitResult.Fail("Invalid garment selection."));

    var result = await matching.SaveOutfitSuggestionAsync(
        request.UserId,
        request.Recommendation,
        request.SeedGarmentIds ?? Array.Empty<string>());
    return Results.Ok(result);
})
.WithName("MobileSaveMatch")
.WithTags("Mobile API");

// Mobile outfit wear log:
// stores that the selected saved outfit was just worn, using the server/database timestamp.
// Topic: Mobile mark outfit worn endpoint
// Purpose: Lets MAUI record an outfit-only worn timestamp for a customer or admin-selected customer.
// Search keywords: API MOBILE OUTFIT_WEAR_LOG OUTFIT ADD TIMESTAMP ADMIN
// When to use it: Show this when explaining how MAUI writes outfit usage history through the API.
// Important notes: This endpoint does not edit/delete outfits; it only inserts into outfit_wear_logs.
// FLOW_OUTFIT_WEAR_MOBILE_04: Program.cs checks actor access and outfit ownership before writing the wear log.
// This file is involved because MAUI must write through API, not directly to MySQL; next step is OutfitWearLogDB.MarkWornAsync.
app.MapPost("/api/mobile/outfits/{outfitId}/wear", async (
    string outfitId,
    MobileOutfitWearRequest request,
    UserDB userDb,
    OutfitDB outfitDb,
    OutfitWearLogDB outfitWearLogDb) =>
{
    // VALIDATION_OUTFIT_ID / VALIDATION_USER_ID: wear logging validates outfit, actor, and optional target ids before DB write.
    if (!IsValidGuidText(outfitId) ||
        !IsValidGuidText(request.ActorUserId) ||
        (!string.IsNullOrWhiteSpace(request.UserId) && !IsValidGuidText(request.UserId)))
    {
        return Results.BadRequest(new MobileBasicResponse(false, "Invalid outfit or user id."));
    }

    var actor = await userDb.GetByIdAsync(request.ActorUserId);
    if (actor == null)
        return Results.BadRequest(new MobileBasicResponse(false, "actorUserId is required."));

    if (IsAdmin(actor))
        return Results.Forbid();

    var targetUserId = string.IsNullOrWhiteSpace(request.UserId)
        ? actor.UserId
        : request.UserId.Trim();

    if (!CanAccessUserData(actor, targetUserId))
        return Results.Forbid();

    var owned = await outfitDb.GetByUserAsync(targetUserId, 2000);
    if (!owned.Any(x => string.Equals(x.OutfitId, outfitId, StringComparison.OrdinalIgnoreCase)))
        return Results.NotFound(new MobileBasicResponse(false, "Outfit was not found for this user."));

    await outfitWearLogDb.MarkWornAsync(targetUserId, outfitId);
    return Results.Ok(new MobileBasicResponse(true, "Outfit marked as worn."));
})
.WithName("MobileMarkOutfitWorn")
.WithTags("Mobile API");

// Mobile outfit delete removes link rows first, then the outfit row.
// FLOW_DELETE_OUTFIT_MOBILE_04: Program.cs verifies the outfit belongs to the user, deletes links, then deletes the outfit.
// This file is involved because saved outfit deletion must clean child rows first; next step is OutfitGarmentDB then OutfitDB.
app.MapDelete("/api/mobile/outfits/{outfitId}", async (
    string outfitId,
    string userId,
    OutfitDB outfitDb,
    OutfitGarmentDB outfitGarmentDb) =>
{
    // VALIDATION_OUTFIT_ID / VALIDATION_USER_ID: outfit delete validates ids before ownership and delete work.
    if (!IsValidGuidText(outfitId) || !IsValidGuidText(userId))
        return Results.BadRequest("Invalid outfit or user id.");

    var owned = await outfitDb.GetByUserAsync(userId, 2000);
    if (!owned.Any(x => string.Equals(x.OutfitId, outfitId, StringComparison.OrdinalIgnoreCase)))
        return Results.Forbid();

    await outfitGarmentDb.DeleteByOutfitIdAsync(outfitId);
    var deleted = await outfitDb.DeleteOutfitAsync(outfitId);
    return deleted > 0 ? Results.NoContent() : Results.Problem("Could not delete outfit.");
})
.WithName("MobileDeleteOutfit")
.WithTags("Mobile API");

// Blazor page routing starts here after API endpoints are mapped.
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Logs whether garment analysis uses config prompt, prompt file, or built-in fallback.
static void LogGarmentPromptConfiguration(WebApplication app, ILogger logger)
{
    var promptFromConfig = app.Configuration[GarmentFeatureService.PromptConfigKey];
    var promptFilePath = ResolvePromptPath(
        app.Environment.ContentRootPath,
        app.Configuration[GarmentFeatureService.PromptFileConfigKey]);

    if (!string.IsNullOrWhiteSpace(promptFromConfig))
    {
        logger.LogInformation(
            "Garment image analysis prompt loaded from config key {PromptKey}.",
            GarmentFeatureService.PromptConfigKey);
        return;
    }

    if (File.Exists(promptFilePath))
    {
        logger.LogInformation(
            "Garment image analysis prompt loaded from file {PromptFile}.",
            promptFilePath);
        return;
    }

    logger.LogWarning(
        "Garment image analysis prompt is missing. Expected config key {PromptKey} or prompt file key {PromptFileKey} (resolved path {PromptFile}). Built-in prompt fallback will be used.",
        GarmentFeatureService.PromptConfigKey,
        GarmentFeatureService.PromptFileConfigKey,
        promptFilePath);
}

// Logs Gemini configuration source without printing the secret key.
static void LogGeminiConfiguration(WebApplication app, ILogger logger)
{
    var model = app.Configuration["Gemini:Model"] ?? "gemini-2.5-flash";
    var keyFromConfig = app.Configuration["Gemini:ApiKey"] ?? app.Configuration["GeminiApiKey"];
    var keyFromEnv = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
        ?? Environment.GetEnvironmentVariable("GOOGLE_API_KEY")
        ?? Environment.GetEnvironmentVariable("GEMINI_KEY")
        ?? Environment.GetEnvironmentVariable("GENAI_API_KEY");

    if (!string.IsNullOrWhiteSpace(keyFromEnv))
    {
        logger.LogInformation("Gemini configured via environment variable. Model={Model}", model);
        return;
    }

    if (!string.IsNullOrWhiteSpace(keyFromConfig))
    {
        logger.LogInformation("Gemini configured via app configuration/user-secrets. Model={Model}", model);
        return;
    }

    logger.LogWarning("Gemini API key not found. Set Gemini:ApiKey in User Secrets or GEMINI_API_KEY/GOOGLE_API_KEY in environment.");
}

// Resolves prompt file paths from appsettings/secrets into absolute paths.
static string ResolvePromptPath(string contentRootPath, string? configuredPath)
{
    var value = string.IsNullOrWhiteSpace(configuredPath)
        ? "Prompts/garment-features.txt"
        : configuredPath.Trim();

    if (Path.IsPathRooted(value))
        return value;

    return Path.Combine(contentRootPath, value.Replace('/', Path.DirectorySeparatorChar));
}

// Converts query-string values into the garment filter request used by GarmentDB.
static Models.GarmentFilterRequest ParseGarmentFilterRequest(IQueryCollection query)
{
    return new Models.GarmentFilterRequest
    {
        Categories = ReadList(query, "categories"),
        Colors = ReadList(query, "colors"),
        Seasons = ReadList(query, "seasons"),
        Occasions = ReadList(query, "occasions"),
        Brands = ReadList(query, "brands")
    };
}

// Converts query-string values into the outfit filter request used by OutfitDB.
static Models.OutfitFilterRequest ParseOutfitFilterRequest(IQueryCollection query)
{
    var min = 40;
    var max = 100;
    if (int.TryParse(query["minScore"], out var parsedMin)) min = parsedMin;
    if (int.TryParse(query["maxScore"], out var parsedMax)) max = parsedMax;

    return new Models.OutfitFilterRequest
    {
        MinScore = min,
        MaxScore = max,
        StyleLabels = ReadList(query, "styleLabels"),
        Seasons = ReadList(query, "seasons"),
        RecommendedPlaces = ReadList(query, "recommendedPlaces")
    };
}

// Reads repeated/comma-separated query values into normalized distinct strings.
static List<string> ReadList(IQueryCollection query, string key)
{
    var raw = query[key];
    if (raw.Count == 0)
        return new List<string>();

    return raw
        .SelectMany(x => (x ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Select(x => x.Trim().ToLowerInvariant())
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();
}

// Supports both Smtp section settings and older SMTP_* environment variable names.
static void ApplyLegacySmtpOverrides(SmtpOptions options, IConfiguration configuration)
{
    options.Host = FirstNonEmpty(options.Host, configuration["SMTP_HOST"]);
    options.Username = FirstNonEmpty(options.Username, configuration["SMTP_USER"]);
    options.Password = FirstNonEmpty(options.Password, configuration["SMTP_PASS"]);
    options.FromEmail = FirstNonEmpty(options.FromEmail, configuration["SMTP_FROM"]);
    options.FromName = FirstNonEmpty(options.FromName, configuration["SMTP_FROM_NAME"]);

    var portText = FirstNonEmpty(configuration["SMTP_PORT"], configuration["SMTP_POR"]);
    if (!string.IsNullOrWhiteSpace(portText) && int.TryParse(portText, out var port) && port > 0)
    {
        options.Port = port;
    }

    var secureText = configuration["SMTP_SECURE"];
    if (TryParseBool(secureText, out var secure))
    {
        options.EnableSsl = secure;
    }

    if (string.IsNullOrWhiteSpace(options.FromEmail) && !string.IsNullOrWhiteSpace(options.Username))
    {
        options.FromEmail = options.Username.Trim();
    }
}

// Returns the first configured SMTP value that is not empty.
static string FirstNonEmpty(params string?[] values)
{
    foreach (var value in values)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value.Trim();
        }
    }

    return string.Empty;
}

// Parses flexible boolean config values such as true/false, yes/no, and 1/0.
static bool TryParseBool(string? value, out bool result)
{
    if (bool.TryParse(value, out result))
    {
        return true;
    }

    var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
    if (normalized is "1" or "yes" or "y" or "on")
    {
        result = true;
        return true;
    }

    if (normalized is "0" or "no" or "n" or "off")
    {
        result = false;
        return true;
    }

    result = false;
    return false;
}

// Access helpers shared by mobile admin endpoints.
static bool IsAdmin(User? user) =>
    user?.Role?.Equals("admin", StringComparison.OrdinalIgnoreCase) == true;

// Allows users to access their own data and admins to access any customer's data.
static bool CanAccessUserData(User actor, string targetUserId) =>
    string.Equals(actor.UserId, targetUserId, StringComparison.OrdinalIgnoreCase) || IsAdmin(actor);

// Converts the MAUI emulator API URL into a reset-link URL that can open from the computer browser.
// Android uses 10.0.2.2 to reach the host PC, but Windows browsers should use localhost instead.
static string ResolvePasswordResetOrigin(string? requestedOrigin, HttpRequest httpRequest, IConfiguration configuration)
{
    var configuredOrigin = configuration["PasswordReset:PublicBaseUrl"];
    var origin = !string.IsNullOrWhiteSpace(configuredOrigin)
        ? configuredOrigin.Trim()
        : !string.IsNullOrWhiteSpace(requestedOrigin)
            ? requestedOrigin.Trim()
            : $"{httpRequest.Scheme}://{httpRequest.Host}/";

    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        return origin;

    var isEmulatorOnlyHost =
        string.Equals(uri.Host, "10.0.2.2", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase);

    if (!isEmulatorOnlyHost)
        return uri.ToString();

    var builder = new UriBuilder(uri)
    {
        Host = "localhost"
    };

    return builder.Uri.ToString();
}

// Deletes a user and all dependent wardrobe data in dependency order.
// Used by admin account deletion and self-delete account flows.
static async Task<int> DeleteUserCascadeAsync(
    string userId,
    UserDB userDb,
    GarmentDB garmentDb,
    OutfitDB outfitDb,
    OutfitGarmentDB outfitGarmentDb)
{
    var outfits = await outfitDb.GetByUserAsync(userId, 2000);
    foreach (var outfit in outfits)
    {
        await outfitGarmentDb.DeleteByOutfitIdAsync(outfit.OutfitId);
        await outfitDb.DeleteOutfitAsync(outfit.OutfitId);
    }

    var garments = await garmentDb.GetByUserAsync(userId);
    foreach (var garment in garments)
        await garmentDb.DeleteGarmentAsync(garment.GarmentId);

    return await userDb.DeleteUserAsync(userId);
}

// Builds mobile dashboard chart data for the same counts shown on the web admin home.
static IReadOnlyList<MobileUsagePoint> BuildMobileUsagePoints(
    int days,
    Dictionary<DateTime, int> usersByDay,
    Dictionary<DateTime, int> garmentsByDay,
    Dictionary<DateTime, int> outfitsByDay)
{
    var today = DateTime.UtcNow.Date;
    var start = today.AddDays(-(days - 1));
    var points = new List<MobileUsagePoint>(days);

    for (var i = 0; i < days; i++)
    {
        var day = start.AddDays(i).Date;
        points.Add(new MobileUsagePoint(
            day,
            usersByDay.GetValueOrDefault(day),
            garmentsByDay.GetValueOrDefault(day),
            outfitsByDay.GetValueOrDefault(day)));
    }

    return points;
}

// Topic: Apply outfit wear summaries to API models
// Purpose: Copies count, first worn, and last worn values onto Outfit objects before returning JSON.
// Search keywords: API MOBILE OUTFIT_WEAR_LOG OUTFIT HISTORY COUNT TIMESTAMP
// When to use it: Use when the mobile outfit endpoint needs to show wear summary on cards.
// Important notes: This only enriches response models; it does not update the outfits table.
// FLOW_OUTFIT_WEAR_STATS_MOBILE_04: Program.cs copies DB summary values onto Outfit objects for MAUI JSON.
// This file is involved because the mobile API response must include summary data; next step is MAUI card rendering.
static void ApplyOutfitWearSummaries(IEnumerable<Outfit> outfits, IReadOnlyDictionary<string, OutfitWearLog> summaries)
{
    foreach (var outfit in outfits)
    {
        if (summaries.TryGetValue(outfit.OutfitId, out var summary))
        {
            outfit.WearCount = summary.WearCount;
            outfit.FirstWornAt = summary.FirstWornAt;
            outfit.LastWornAt = summary.LastWornAt;
        }
        else
        {
            outfit.WearCount = 0;
            outfit.FirstWornAt = null;
            outfit.LastWornAt = null;
        }
    }
}

// Keeps role storage limited to admin/customer.
static string NormalizeRole(string? role)
{
    var normalized = (role ?? string.Empty).Trim().ToLowerInvariant();
    return normalized == "admin" ? "admin" : "customer";
}

// Topic: API input validation helpers
// Purpose: Centralizes validation for values that users send from MAUI/mobile API requests.
// Search keywords: VALIDATION_EMAIL VALIDATION_PASSWORD VALIDATION_NAME VALIDATION_ROLE VALIDATION_USER_ID VALIDATION_IMAGE VALIDATION_PROMPT
// When to use it: Use before database lookup/insert/update or AI processing in mobile endpoints.
// Important notes: Web forms also have DataAnnotations; API endpoints must still validate because clients can bypass UI.
static bool IsValidEmail(string? email)
{
    // VALIDATION_EMAIL: checks that a user-supplied email has a normal email address shape before DB/account work.
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

static bool IsValidPassword(string? password)
{
    // VALIDATION_PASSWORD: checks that a user-supplied password meets the project minimum length.
    return !string.IsNullOrWhiteSpace(password) && password.Length >= 6 && password.Length <= 128;
}

static bool IsValidGuidText(string? value)
{
    // VALIDATION_USER_ID / VALIDATION_GARMENT_ID / VALIDATION_OUTFIT_ID: checks ids are GUID-shaped before DB access.
    return Guid.TryParse(value, out _);
}

static bool IsValidImageMimeType(string? mimeType)
{
    // VALIDATION_IMAGE_MIME: allows only image MIME types the upload flow is expected to handle.
    var normalized = (mimeType ?? string.Empty).Trim().ToLowerInvariant();
    return normalized is "image/jpeg" or "image/jpg" or "image/png" or "image/webp" or "image/gif";
}

static bool IsValidFileName(string? fileName)
{
    // VALIDATION_IMAGE_FILENAME: rejects empty/overlong names and path-like values from upload requests.
    var name = (fileName ?? string.Empty).Trim();
    return name.Length is >= 1 and <= 255 &&
           name.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
           !name.Contains('/') &&
           !name.Contains('\\');
}

static bool IsValidPrompt(string? prompt)
{
    // VALIDATION_MATCH_PROMPT: caps user-written recommendation prompts so they cannot become oversized API input.
    return (prompt ?? string.Empty).Length <= 500;
}

// Picks the AI value first, then manual/mobile fallback values.
static string? FirstOptional(params string?[] values)
{
    foreach (var value in values)
    {
        if (!string.IsNullOrWhiteSpace(value))
            return value.Trim();
    }

    return null;
}

// Hashes uploaded image bytes so duplicate image detection can be enforced by SQL.
static string Sha256Hex(byte[] bytes)
{
    using var sha = SHA256.Create();
    return Convert.ToHexString(sha.ComputeHash(bytes)).ToLowerInvariant();
}

// Parses Gemini feature JSON into the fields stored on the garment row.
static MobileGarmentFeatures ParseMobileFeatures(string json)
{
    try
    {
        var doc = JsonDocument.Parse(json ?? "{}");
        var root = doc.RootElement;

        string? ReadString(string name) =>
            root.TryGetProperty(name, out var value) && value.ValueKind != JsonValueKind.Null
                ? value.GetString()
                : null;

        int? formalityLevel = null;
        if (root.TryGetProperty("formality_level", out var formality))
        {
            if (formality.ValueKind == JsonValueKind.Number && formality.TryGetInt32(out var number))
                formalityLevel = number;
            else if (formality.ValueKind == JsonValueKind.String && int.TryParse(formality.GetString(), out var parsed))
                formalityLevel = parsed;
        }

        string? styleTags = null;
        if (root.TryGetProperty("style_tags", out var tags) && tags.ValueKind == JsonValueKind.Array)
        {
            var values = tags.EnumerateArray()
                .Select(x => x.GetString())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (values.Count > 0)
                styleTags = JsonSerializer.Serialize(values);
        }

        var rawType = ReadString("type");
        return new MobileGarmentFeatures(
            NormalizeGarmentType(rawType, json),
            ReadString("color"),
            ReadString("color_secondary"),
            ReadString("pattern"),
            ReadString("style_category"),
            ReadString("season"),
            ReadString("occasion"),
            formalityLevel,
            styleTags,
            ReadString("fit"),
            ReadString("material"),
            ReadString("sleeve"),
            ReadString("length"),
            ReadString("brand"));
    }
    catch
    {
        return MobileGarmentFeatures.Empty;
    }
}

// Converts AI/mobile type wording into the three project categories used by matching.
static string NormalizeGarmentType(string? raw, string? fullText = null)
{
    // Local helper used by NormalizeGarmentType to match one text source.
    // It checks direct AI/mobile type text first, then the full JSON/text fallback.
    static string Match(string? value)
    {
        var source = (value ?? string.Empty).Trim().ToLowerInvariant();
        if (source.Contains("shirt") || source.Contains("t-shirt") || source.Contains("tee") || source.Contains("top"))
            return "shirt";
        if (source.Contains("pant") || source.Contains("trouser") || source.Contains("jean") || source.Contains("bottom"))
            return "pants";
        if (source.Contains("shoe") || source.Contains("sneaker") || source.Contains("boot") || source.Contains("foot"))
            return "shoes";
        return string.Empty;
    }

    var direct = Match(raw);
    if (!string.IsNullOrWhiteSpace(direct))
        return direct;

    return Match(fullText);
}

// Internal parsed feature bag used only while handling mobile garment upload.
internal sealed record MobileGarmentFeatures(
    string Type,
    string? Color,
    string? ColorSecondary,
    string? Pattern,
    string? StyleCategory,
    string? Season,
    string? Occasion,
    int? FormalityLevel,
    string? StyleTags,
    string? Fit,
    string? Material,
    string? Sleeve,
    string? Length,
    string? Brand)
{
    public static MobileGarmentFeatures Empty { get; } = new(
        string.Empty,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null);
}
