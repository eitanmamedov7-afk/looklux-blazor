// מה הקובץ עושה: הקובץ מרכז חלק מהמערכת ומשתתף בהפעלת הפרויקט.
// למה הקובץ נדרש: הוא נדרש כדי שהחלק הזה בפרויקט יפעל בצורה ברורה ומסודרת.
// לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר למסכים, לשירותים, למודלים ולשכבת הדיבי לפי השימוש שלו.
// איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים לקבצים שמזמנים את הקוד הזה או לקבצים שהוא מזמן.

// מה הקובץ עושה: הקובץ מסביר את התפקיד של החלק הזה בפרויקט.
// למה הקובץ נדרש: הוא עוזר לחבר בין הקוד, התצוגה והנתונים.
// לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר לדפים, לשירותים, למודלם ולדיבי לפי הצורך.
// איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים בקבצים שקוראים לקובץ הזה או בקבצים שהוא קורא להמ.



// ייבוא ספריות שמספקות מחלקות, ממשקים ופעולות שהקובץ צריך כדי לעבוד.
using gadifff.Components;
using DBL;
using gadifff.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;

// יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("secrets.json", optional: true, reloadOnChange: true);

// בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
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
builder.Services.AddScoped<UserMatchingStateDB>();

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

// יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
var app = builder.Build();
// יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
LogGarmentPromptConfiguration(app, startupLogger);
LogGeminiConfiguration(app, startupLogger);

// בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
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
    // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
    var img = await imgDb.GetLatestByGarmentIdAsync(garmentId);
    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (img == null) return Results.NotFound();

    // החזרת התוצאה אל הקוד שקרא לפעולה.
    return Results.File(img.Value.Bytes, img.Value.MimeType);
})
.WithName("GetGarmentImageByGarmentId")
.WithTags("Media");

app.MapGet("/api/closet/items", async (HttpRequest request, GarmentDB garmentDb) =>
{
    // קריאה לשכבת הדיבי כדי לשלוף, לשמור, לעדכן או למחוק מידע ששייך למשתמש.
    var userId = request.Query["userId"].ToString();
    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (string.IsNullOrWhiteSpace(userId))
        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return Results.BadRequest("userId is required.");

    // קריאה לשכבת הדיבי כדי לשלוף, לשמור, לעדכן או למחוק מידע ששייך למשתמש.
    var filter = ParseGarmentFilterRequest(request.Query);
    // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
    var items = await garmentDb.GetFilteredByUserAsync(userId, filter, 1000);
    // החזרת התוצאה אל הקוד שקרא לפעולה.
    return Results.Ok(items);
})
.WithName("GetClosetItems")
.WithTags("Internal API");

app.MapGet("/api/closet/filters", async (HttpRequest request, GarmentDB garmentDb) =>
{
    // קריאה לשכבת הדיבי כדי לשלוף, לשמור, לעדכן או למחוק מידע ששייך למשתמש.
    var userId = request.Query["userId"].ToString();
    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (string.IsNullOrWhiteSpace(userId))
        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return Results.BadRequest("userId is required.");

    // קריאה לשכבת הדיבי כדי לשלוף, לשמור, לעדכן או למחוק מידע ששייך למשתמש.
    var filter = ParseGarmentFilterRequest(request.Query);
    // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
    var options = await garmentDb.GetFilterOptionsAsync(userId, filter);
    // החזרת התוצאה אל הקוד שקרא לפעולה.
    return Results.Ok(options);
})
.WithName("GetClosetFilters")
.WithTags("Internal API");

app.MapGet("/api/outfits/items", async (HttpRequest request, OutfitDB outfitDb) =>
{
    // קריאה לשכבת הדיבי כדי לשלוף, לשמור, לעדכן או למחוק מידע ששייך למשתמש.
    var userId = request.Query["userId"].ToString();
    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (string.IsNullOrWhiteSpace(userId))
        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return Results.BadRequest("userId is required.");

    // קריאה לשכבת הדיבי כדי לשלוף, לשמור, לעדכן או למחוק מידע ששייך למשתמש.
    var filter = ParseOutfitFilterRequest(request.Query);
    // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
    var outfits = await outfitDb.GetFilteredByUserAsync(userId, filter, 400);
    // החזרת התוצאה אל הקוד שקרא לפעולה.
    return Results.Ok(outfits);
})
.WithName("GetOutfitItems")
.WithTags("Internal API");

app.MapGet("/api/outfits/filters", async (HttpRequest request, OutfitDB outfitDb) =>
{
    // קריאה לשכבת הדיבי כדי לשלוף, לשמור, לעדכן או למחוק מידע ששייך למשתמש.
    var userId = request.Query["userId"].ToString();
    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (string.IsNullOrWhiteSpace(userId))
        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return Results.BadRequest("userId is required.");

    // קריאה לשכבת הדיבי כדי לשלוף, לשמור, לעדכן או למחוק מידע ששייך למשתמש.
    var filter = ParseOutfitFilterRequest(request.Query);
    // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
    var options = await outfitDb.GetFilterOptionsAsync(userId, filter);
    // החזרת התוצאה אל הקוד שקרא לפעולה.
    return Results.Ok(options);
})
.WithName("GetOutfitFilters")
.WithTags("Internal API");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static void LogGarmentPromptConfiguration(WebApplication app, ILogger logger)
{
    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
    var promptFromConfig = app.Configuration[GarmentFeatureService.PromptConfigKey];
    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
    var promptFilePath = ResolvePromptPath(
        app.Environment.ContentRootPath,
        app.Configuration[GarmentFeatureService.PromptFileConfigKey]);

    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (!string.IsNullOrWhiteSpace(promptFromConfig))
    {
        logger.LogInformation(
            "Garment image analysis prompt loaded from config key {PromptKey}.",
            GarmentFeatureService.PromptConfigKey);
        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return;
    }

    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (File.Exists(promptFilePath))
    {
        logger.LogInformation(
            "Garment image analysis prompt loaded from file {PromptFile}.",
            promptFilePath);
        // החזרת התוצאה אל הקוד שקרא לפעולה.
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
    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
    var model = app.Configuration["Gemini:Model"] ?? "gemini-2.5-flash";
    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
    var keyFromConfig = app.Configuration["Gemini:ApiKey"] ?? app.Configuration["GeminiApiKey"];
    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
    var keyFromEnv = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
        ?? Environment.GetEnvironmentVariable("GOOGLE_API_KEY")
        ?? Environment.GetEnvironmentVariable("GEMINI_KEY")
        ?? Environment.GetEnvironmentVariable("GENAI_API_KEY");

    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (!string.IsNullOrWhiteSpace(keyFromEnv))
    {
        logger.LogInformation("Gemini configured via environment variable. Model={Model}", model);
        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return;
    }

    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (!string.IsNullOrWhiteSpace(keyFromConfig))
    {
        logger.LogInformation("Gemini configured via app configuration/user-secrets. Model={Model}", model);
        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return;
    }

    logger.LogWarning("Gemini API key not found. Set Gemini:ApiKey in User Secrets or GEMINI_API_KEY/GOOGLE_API_KEY in environment.");
}

static string ResolvePromptPath(string contentRootPath, string? configuredPath)
{
    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
    var value = string.IsNullOrWhiteSpace(configuredPath)
        ? "Prompts/garment-features.txt"
        : configuredPath.Trim();

    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (Path.IsPathRooted(value))
        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return value;

    // החזרת התוצאה אל הקוד שקרא לפעולה.
    return Path.Combine(contentRootPath, value.Replace('/', Path.DirectorySeparatorChar));
}

static Models.GarmentFilterRequest ParseGarmentFilterRequest(IQueryCollection query)
{
    // החזרת התוצאה אל הקוד שקרא לפעולה.
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
    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
    var min = 40;
    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
    var max = 100;
    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (int.TryParse(query["minScore"], out var parsedMin)) min = parsedMin;
    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (int.TryParse(query["maxScore"], out var parsedMax)) max = parsedMax;

    // החזרת התוצאה אל הקוד שקרא לפעולה.
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
    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
    var raw = query[key];
    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (raw.Count == 0)
        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return new List<string>();

    // החזרת התוצאה אל הקוד שקרא לפעולה.
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

    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
    var portText = FirstNonEmpty(configuration["SMTP_PORT"], configuration["SMTP_POR"]);
    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (!string.IsNullOrWhiteSpace(portText) && int.TryParse(portText, out var port) && port > 0)
    {
        options.Port = port;
    }

    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
    var secureText = configuration["SMTP_SECURE"];
    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (TryParseBool(secureText, out var secure))
    {
        options.EnableSsl = secure;
    }

    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (string.IsNullOrWhiteSpace(options.FromEmail) && !string.IsNullOrWhiteSpace(options.Username))
    {
        options.FromEmail = options.Username.Trim();
    }
}

static string FirstNonEmpty(params string?[] values)
{
    // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
    foreach (var value in values)
    {
        // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if (!string.IsNullOrWhiteSpace(value))
        {
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return value.Trim();
        }
    }

    // החזרת התוצאה אל הקוד שקרא לפעולה.
    return string.Empty;
}

static bool TryParseBool(string? value, out bool result)
{
    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (bool.TryParse(value, out result))
    {
        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return true;
    }

    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
    var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (normalized is "1" or "yes" or "y" or "on")
    {
        result = true;
        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return true;
    }

    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
    if (normalized is "0" or "no" or "n" or "off")
    {
        result = false;
        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return true;
    }

    result = false;
    // החזרת התוצאה אל הקוד שקרא לפעולה.
    return false;
}
