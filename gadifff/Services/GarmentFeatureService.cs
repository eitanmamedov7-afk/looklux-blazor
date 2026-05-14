// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace gadifff.Services
{
    /// <summary>
    /// Extracts structured garment features via Gemini. Sends image bytes in prompt as base64.
    /// </summary>
    public class GarmentFeatureService
    {
        public const string PromptConfigKey = "GarmentFeature:ImageAnalysisPrompt";
        public const string PromptFileConfigKey = "GarmentFeature:ImageAnalysisPromptFile";

        private const string LegacyPromptConfigKey = "Gemini:ImageAnalysisPrompt";
        private const string DefaultPromptFileRelativePath = "Prompts/garment-features.txt";

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

        private readonly GeminiClient _gemini;
        private readonly ILogger<GarmentFeatureService> _logger;
        private readonly IWebHostEnvironment _env;
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

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public async Task<string> ExtractFromImageAsync(byte[] imageBytes, string fileName, string contentType)
        {
            if (!_gemini.IsConfigured)
            {
                var msg = "Image analysis is not configured (set GEMINI_API_KEY or Gemini:ApiKey).";
                _logger.LogError(msg);
                throw new InvalidOperationException(msg);
            }

            var (prompt, source) = await LoadPromptAsync();
            if (string.IsNullOrWhiteSpace(prompt))
            {
                _logger.LogWarning(
                    "Garment feature prompt missing after all fallbacks. Expected key {PromptKey} or file key {PromptFileKey}.",
                    PromptConfigKey,
                    PromptFileConfigKey);
                throw new InvalidOperationException("Image analysis prompt missing on server.");
            }

            if (string.Equals(source, "built-in", StringComparison.Ordinal))
            {
                _logger.LogWarning(
                    "Using built-in garment analysis prompt. Configure {PromptKey} or {PromptFileKey} to override.",
                    PromptConfigKey,
                    PromptFileConfigKey);
            }

            var composedPrompt = $"{prompt}\nImageFileName: {fileName}";
            var raw = await _gemini.GenerateAsync(composedPrompt, imageBytes, contentType);
            var normalizedRaw = NormalizeJson(raw);

            if (string.IsNullOrWhiteSpace(normalizedRaw))
            {
                _logger.LogWarning("Gemini returned empty; returning empty json");
                throw new InvalidOperationException("Image analysis returned no content.");
            }

            // Validate strict JSON; if invalid, fall back
            try
            {
                JsonDocument.Parse(normalizedRaw);
                return normalizedRaw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Gemini feature JSON parse failed. Raw (trimmed): {Sample}",
                    normalizedRaw?.Substring(0, Math.Min(200, normalizedRaw.Length)));
                throw new InvalidOperationException("Image analysis returned invalid JSON.", ex);
            }
        }

        private async Task<(string Prompt, string Source)> LoadPromptAsync()
        {
            var promptFromConfig = _config[PromptConfigKey];
            if (!string.IsNullOrWhiteSpace(promptFromConfig))
                return (promptFromConfig, PromptConfigKey);

            var promptFromLegacyConfig = _config[LegacyPromptConfigKey];
            if (!string.IsNullOrWhiteSpace(promptFromLegacyConfig))
                return (promptFromLegacyConfig, LegacyPromptConfigKey);

            var promptPath = ResolvePromptPath(_config[PromptFileConfigKey]);

            try
            {
                if (File.Exists(promptPath))
                {
                    var promptText = await File.ReadAllTextAsync(promptPath);
                    if (!string.IsNullOrWhiteSpace(promptText))
                        return (promptText, promptPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read garment feature prompt file at {PromptPath}", promptPath);
            }

            return (BuiltInPrompt, "built-in");
        }

        // הסבר: פונקציית נרמול. מנקה ומאחד פורמט נתונים כדי למנוע חוסר עקביות בהמשך הזרימה.
        private static string NormalizeJson(string? raw)
        {
            var value = (raw ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            if (value.StartsWith("```", StringComparison.Ordinal))
            {
                value = value.Trim('`').Trim();
                if (value.StartsWith("json", StringComparison.OrdinalIgnoreCase))
                    value = value[4..].Trim();
            }

            var first = value.IndexOf('{');
            var last = value.LastIndexOf('}');
            if (first >= 0 && last > first)
                value = value[first..(last + 1)];

            return value.Trim();
        }

        // הסבר: פונקציית resolve. מחליטה מה הערך/המקור הנכון לשימוש לפי סדר עדיפויות ברור.
        private string ResolvePromptPath(string? configuredPath)
        {
            var value = string.IsNullOrWhiteSpace(configuredPath)
                ? DefaultPromptFileRelativePath
                : configuredPath.Trim();

            if (Path.IsPathRooted(value))
                return value;

            return Path.Combine(_env.ContentRootPath, value.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
