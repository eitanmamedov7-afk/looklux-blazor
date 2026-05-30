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
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// הגדרת מרחו שמות שממקם את הקובץ בטבקת הפרויקט המטאימה.
namespace gadifff.Services
{
    // הגדרת מבנה מרכזי שמרכז נתונים או פעוליות עובר החלק הזה בפרויקט.
    public class GarmentFeatureService
    {
        // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
        public const string PromptConfigKey = "GarmentFeature:ImageAnalysisPrompt";
        // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
        public const string PromptFileConfigKey = "GarmentFeature:ImageAnalysisPromptFile";

        // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
        private const string LegacyPromptConfigKey = "Gemini:ImageAnalysisPrompt";
        // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
        private const string DefaultPromptFileRelativePath = "Prompts/garment-features.txt";

        // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
        private static readonly string BuiltInPrompt = """
You are a clothing image analyzer.
Return ONLY valid JSON (no markdown, no explanations) with this exact shape:
{
  "type": "shirt|pants|shoes|",
  "color": "short color name or null",
  "color_secondary": "second color or null",
  "pattern": "solid|striped|checked|graphic|floral|other|null",
  "style_category": "casual|smart_casual|formal|sporty|streetwear|null",
  "season": "summer|winter|transitional|all|null",
  "occasion": "gym|work|date|daily|event|null",
  "formality_level": "1-5 or null",
  "style_tags": ["tag1", "tag2"],
  "fit": "fit or null",
  "material": "material or null",
  "sleeve": "sleeve or null",
  "length": "length or null",
  "brand": "brand or null"
}
Rules:
- If uncertain, keep fields null or empty string.
- Keep "type" limited to shirt, pants, shoes, or empty string.
- Keep "formality_level" as integer 1-5 only, otherwise null.
- Keep "style_tags" concise and fashion-focused.
- Output JSON only.
""";

        // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
        private readonly GeminiClient _gemini;
        // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
        private readonly ILogger<GarmentFeatureService> _logger;
        // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
        private readonly IWebHostEnvironment _env;
        // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
        private readonly IConfiguration _config;

        public GarmentFeatureService(
            GeminiClient gemini,
            ILogger<GarmentFeatureService> logger,
            IWebHostEnvironment env,
            IConfiguration config)
        {
            _gemini = gemini;
            _logger = logger;
            _env = env;
            _config = config;
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<string> ExtractFromImageAsync(byte[] imageBytes, string fileName, string contentType)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (!_gemini.IsConfigured)
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var msg = "Image analysis is not configured (set GEMINI_API_KEY or Gemini:ApiKey).";
                _logger.LogError(msg);
                throw new InvalidOperationException(msg);
            }

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var (prompt, source) = await LoadPromptAsync();
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (string.IsNullOrWhiteSpace(prompt))
            {
                _logger.LogWarning(
                    "Garment feature prompt missing after all fallbacks. Expected key {PromptKey} or file key {PromptFileKey}.",
                    PromptConfigKey,
                    PromptFileConfigKey);
                throw new InvalidOperationException("Image analysis prompt missing on server.");
            }

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (string.Equals(source, "built-in", StringComparison.Ordinal))
            {
                _logger.LogWarning(
                    "Using built-in garment analysis prompt. Configure {PromptKey} or {PromptFileKey} to override.",
                    PromptConfigKey,
                    PromptFileConfigKey);
            }

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var composedPrompt = $"{prompt}\nImageFileName: {fileName}";
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var raw = await _gemini.GenerateAsync(composedPrompt, imageBytes, contentType);
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var normalizedRaw = NormalizeJson(raw);

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (string.IsNullOrWhiteSpace(normalizedRaw))
            {
                _logger.LogWarning("Gemini returned empty; returning empty json");
                throw new InvalidOperationException("Image analysis returned no content.");
            }

            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                JsonDocument.Parse(normalizedRaw);
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return normalizedRaw;
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Gemini feature JSON parse failed. Raw (trimmed): {Sample}",
                    normalizedRaw?.Substring(0, Math.Min(200, normalizedRaw.Length)));
                throw new InvalidOperationException("Image analysis returned invalid JSON.", ex);
            }
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        private async Task<(string Prompt, string Source)> LoadPromptAsync()
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var promptFromConfig = _config[PromptConfigKey];
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (!string.IsNullOrWhiteSpace(promptFromConfig))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return (promptFromConfig, PromptConfigKey);

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var promptFromLegacyConfig = _config[LegacyPromptConfigKey];
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (!string.IsNullOrWhiteSpace(promptFromLegacyConfig))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return (promptFromLegacyConfig, LegacyPromptConfigKey);

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var promptPath = ResolvePromptPath(_config[PromptFileConfigKey]);

            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (File.Exists(promptPath))
                {
                    // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                    var promptText = await File.ReadAllTextAsync(promptPath);
                    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                    if (!string.IsNullOrWhiteSpace(promptText))
                        // החזרת התוצאה אל הקוד שקרא לפעולה.
                        return (promptText, promptPath);
                }
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read garment feature prompt file at {PromptPath}", promptPath);
            }

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return (BuiltInPrompt, "built-in");
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static string NormalizeJson(string? raw)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var value = (raw ?? string.Empty).Trim();
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (string.IsNullOrWhiteSpace(value))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return string.Empty;

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (value.StartsWith("```", StringComparison.Ordinal))
            {
                value = value.Trim('`').Trim();
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (value.StartsWith("json", StringComparison.OrdinalIgnoreCase))
                    value = value[4..].Trim();
            }

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var first = value.IndexOf('{');
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var last = value.LastIndexOf('}');
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (first >= 0 && last > first)
                value = value[first..(last + 1)];

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return value.Trim();
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private string ResolvePromptPath(string? configuredPath)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var value = string.IsNullOrWhiteSpace(configuredPath)
                ? DefaultPromptFileRelativePath
                : configuredPath.Trim();

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (Path.IsPathRooted(value))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return value;

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return Path.Combine(_env.ContentRootPath, value.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
