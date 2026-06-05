



using gadifff.Components;
using DBL;
using gadifff.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("secrets.json", optional: true, reloadOnChange: true);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
}

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddDataProtection();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<UserDB>();

builder.Services.AddScoped<GarmentDB>();
builder.Services.AddScoped<GarmentImageDB>();
builder.Services.AddScoped<OutfitDB>();
builder.Services.AddScoped<OutfitGarmentDB>();

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.PostConfigure<SmtpOptions>(options =>
    ApplyLegacySmtpOverrides(options, builder.Configuration));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<GarmentFeatureService>();
builder.Services.AddScoped<MatchingService>();
builder.Services.AddHttpClient<GeminiClient>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/accessdenied";
    });

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
LogGarmentPromptConfiguration(app, startupLogger);
LogGeminiConfiguration(app, startupLogger);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

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

app.MapGet("/media/garments/by-garment/{garmentId}", async (string garmentId, GarmentImageDB imgDb) =>
{
    var img = await imgDb.GetByGarmentIdAsync(garmentId);
    if (img == null) return Results.NotFound();

    return Results.File(img.Value.Bytes, img.Value.MimeType);
})
.WithName("GetGarmentImageByGarmentId")
.WithTags("Media");

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

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

static string ResolvePromptPath(string contentRootPath, string? configuredPath)
{
    var value = string.IsNullOrWhiteSpace(configuredPath)
        ? "Prompts/garment-features.txt"
        : configuredPath.Trim();

    if (Path.IsPathRooted(value))
        return value;

    return Path.Combine(contentRootPath, value.Replace('/', Path.DirectorySeparatorChar));
}

static Models.GarmentFilterRequest ParseGarmentFilterRequest(IQueryCollection query)
{
    return new Models.GarmentFilterRequest
    {
        Categories = ReadList(query, "categories"),
        Subcategories = ReadList(query, "subcategories"),
        Colors = ReadList(query, "colors"),
        Seasons = ReadList(query, "seasons"),
        Materials = ReadList(query, "materials"),
        Brands = ReadList(query, "brands"),
        Fits = ReadList(query, "fits"),
        Patterns = ReadList(query, "patterns"),
        Tags = ReadList(query, "tags")
    };
}

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
        SeedTypes = ReadList(query, "seedTypes"),
        StyleLabels = ReadList(query, "styleLabels"),
        GarmentTypes = ReadList(query, "garmentTypes"),
        Occasions = ReadList(query, "occasions"),
        Seasons = ReadList(query, "seasons"),
        RecommendedPlaces = ReadList(query, "recommendedPlaces")
    };
}

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
