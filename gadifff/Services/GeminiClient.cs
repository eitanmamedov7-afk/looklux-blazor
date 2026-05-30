// מה הקובץ עושה: הקובץ מרכז חלק מהמערכת ומשתתף בהפעלת הפרויקט.
// למה הקובץ נדרש: הוא נדרש כדי שהחלק הזה בפרויקט יפעל בצורה ברורה ומסודרת.
// לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר למסכים, לשירותים, למודלים ולשכבת הדיבי לפי השימוש שלו.
// איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים לקבצים שמזמנים את הקוד הזה או לקבצים שהוא מזמן.

// הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
// הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
// לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר לדפי בלייזור, למודלם, לדיבי ולשירותים נוספים.
// איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים בדפים שמזריקים את השירות ובקבצי הדיבי שהשירות קורא להם.



// ייבוא ספריות שמספקות מחלקות, ממשקים ופעולות שהקובץ צריך כדי לעבוד.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

// הגדרת מרחו שמות שממקם את הקובץ בטבקת הפרויקט המטאימה.
namespace gadifff.Services
{
    // הגדרת מבנה מרכזי שמרכז נתונים או פעוליות עובר החלק הזה בפרויקט.
    public class GeminiClient
    {
        // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
        private const string DefaultModel = "gemini-2.5-flash";

        // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
        private readonly HttpClient _http;
        // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
        private readonly ILogger<GeminiClient> _logger;
        // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
        private readonly string? _apiKey;
        // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
        private readonly string _model;

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public GeminiClient(HttpClient http, ILogger<GeminiClient> logger, IConfiguration config)
        {
            _http = http;
            _logger = logger;
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var envKey = ResolveApiKey();
            _apiKey = !string.IsNullOrWhiteSpace(envKey) ? envKey : config["Gemini:ApiKey"];

            _model = NormalizeModel(config["Gemini:Model"]);
        }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<string> GenerateAsync(string prompt, byte[]? imageBytes = null, string? mimeType = null, CancellationToken ct = default)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (!IsConfigured)
            {
                _logger.LogWarning("Gemini API key not configured. Using fallback stub response.");
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return "{}";
            }

            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var modelToUse = _model;
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                var (statusCode, isSuccess, body) = await SendRequestAsync(modelToUse, prompt, imageBytes, mimeType, ct);

                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (statusCode == HttpStatusCode.NotFound &&
                    !string.Equals(modelToUse, DefaultModel, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "Gemini model {Model} was not found. Retrying with fallback model {FallbackModel}. Set Gemini:Model to update.",
                        modelToUse,
                        DefaultModel);

                    modelToUse = DefaultModel;
                    // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                    (statusCode, isSuccess, body) = await SendRequestAsync(modelToUse, prompt, imageBytes, mimeType, ct);
                }

                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (!isSuccess)
                {
                    _logger.LogError("Gemini request failed: {Status} {Body}", statusCode, Shorten(body));
                    throw new InvalidOperationException("Gemini call failed");
                }

                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var root = JsonSerializer.Deserialize<JsonElement>(body);
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (root.TryGetProperty("candidates", out var candidates)
                    && candidates.ValueKind == JsonValueKind.Array
                    && candidates.GetArrayLength() > 0)
                {
                    // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
                    foreach (var candidate in candidates.EnumerateArray())
                    {
                        // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                        if (!candidate.TryGetProperty("content", out var content) ||
                            !content.TryGetProperty("parts", out var parts) ||
                            parts.ValueKind != JsonValueKind.Array)
                        {
                            continue;
                        }

                        // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
                        foreach (var part in parts.EnumerateArray())
                        {
                            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                            if (part.ValueKind != JsonValueKind.Object)
                                continue;

                            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                            if (part.TryGetProperty("text", out var textElement) &&
                                textElement.ValueKind == JsonValueKind.String)
                            {
                                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                                var text = textElement.GetString();
                                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                                if (!string.IsNullOrWhiteSpace(text))
                                    // החזרת התוצאה אל הקוד שקרא לפעולה.
                                    return text;
                            }
                        }
                    }
                }

                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (root.TryGetProperty("error", out var error) &&
                    error.ValueKind == JsonValueKind.Object &&
                    error.TryGetProperty("message", out var messageElement) &&
                    messageElement.ValueKind == JsonValueKind.String)
                {
                    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                    var errorMessage = messageElement.GetString();
                    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                    if (!string.IsNullOrWhiteSpace(errorMessage))
                    {
                        _logger.LogWarning("Gemini returned error payload: {Message}", Shorten(errorMessage));
                        // החזרת התוצאה אל הקוד שקרא לפעולה.
                        return errorMessage;
                    }
                }

                _logger.LogWarning("Gemini response missing expected fields. Body: {Body}", Shorten(body));
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return "";
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini request error");
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return "";
            }
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        private async Task<(HttpStatusCode StatusCode, bool IsSuccess, string Body)> SendRequestAsync(string model, string prompt, byte[]? imageBytes, string? mimeType, CancellationToken ct)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = BuildParts(prompt, imageBytes, mimeType)
                    }
                },
                generationConfig = new
                {
                    responseMimeType = "application/json"
                }
            };

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var json = JsonSerializer.Serialize(payload);
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            req.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };

            using var resp = await _http.SendAsync(req, ct);
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var body = await resp.Content.ReadAsStringAsync(ct);
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return (resp.StatusCode, resp.IsSuccessStatusCode, body);
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static object[] BuildParts(string prompt, byte[]? imageBytes, string? mimeType)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var parts = new List<object>
            {
                new { text = prompt }
            };

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (imageBytes?.Length > 0)
            {
                parts.Add(new
                {
                    inline_data = new
                    {
                        mime_type = string.IsNullOrWhiteSpace(mimeType) ? "image/jpeg" : mimeType,
                        data = Convert.ToBase64String(imageBytes)
                    }
                });
            }

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return parts.ToArray();
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static string NormalizeModel(string? configuredModel)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (string.IsNullOrWhiteSpace(configuredModel))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return DefaultModel;

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var model = configuredModel.Trim();
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (model.StartsWith("models/", StringComparison.OrdinalIgnoreCase))
                model = model["models/".Length..];

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (string.Equals(model, "gemini-1.5-flash", StringComparison.OrdinalIgnoreCase))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return DefaultModel;

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return model;
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static string? ResolveApiKey()
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var names = new[]
            {
                "GEMINI_API_KEY",
                "GOOGLE_API_KEY",
                "GEMINI_KEY",
                "GENAI_API_KEY"
            };

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return names
                .Select(Environment.GetEnvironmentVariable)
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static string Shorten(string input, int max = 400)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (string.IsNullOrEmpty(input)) return "";
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return input.Length <= max ? input : input[..max] + "...";
        }
    }
}
