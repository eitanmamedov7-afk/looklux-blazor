
// SEARCH INDEX
// AI, GEMINI, IMAGE, GARMENT, UPLOAD, ANALYZE, JSON, VALIDATE, CONFIG
//
// Topic: GARMENT IMAGE ANALYSIS SERVICE
// Purpose: Sends garment photos to Gemini and returns structured JSON features for saving in garments.
// Search keywords: AI GEMINI IMAGE GARMENT UPLOAD ANALYZE JSON VALIDATE CONFIG
// When to use it: Show this when explaining automatic garment type/color/style extraction.
// Important notes: Used by web upload and mobile upload API before GarmentDB saves the row.

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace gadifff.Services
{
    // SECTION: AI GARMENT FEATURE EXTRACTION
    // Topic: GarmentFeatureService class
    // Purpose: Builds the image-analysis prompt and calls GeminiClient.
    // Search keywords: AI GEMINI IMAGE GARMENT UPLOAD ANALYZE
    // When to use it: Use when explaining the upload pipeline after the image is selected.
    // Important notes: The prompt can come from config, prompt file, or built-in fallback.
    // Service used by Closet upload and the MAUI garment API.
    // It sends garment images to Gemini and returns normalized JSON features that become Garment table fields.
    public class GarmentFeatureService
    {
        // Config keys let the project use a custom prompt from appsettings/user-secrets instead of hard-coded text.
        public const string PromptConfigKey = "GarmentFeature:ImageAnalysisPrompt";
        public const string PromptFileConfigKey = "GarmentFeature:ImageAnalysisPromptFile";

        private const string LegacyPromptConfigKey = "Gemini:ImageAnalysisPrompt";
        private const string DefaultPromptFileRelativePath = "Prompts/garment-features.txt";

        // Built-in fallback prompt keeps upload working in development even if the prompt file is missing.
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
- Keep "style_category" limited to casual, smart_casual, formal, sporty, streetwear, or null.
- Keep "season" limited to summer, winter, transitional, all, or null.
- Keep "occasion" limited to gym, work, date, daily, event, or null.
- Keep "formality_level" as integer 1-5 only, otherwise null.
- Keep "style_tags" concise and fashion-focused.
- Output JSON only.
""";

        // Dependencies:
        // GeminiClient performs the external AI call, config/env decide which prompt is used.
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

        // Main image-analysis flow used after a user chooses a garment photo.
        // Project process affected: garment upload, automatic type detection, and saved garment metadata.
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

        // Loads the prompt in priority order: explicit config, old config key, prompt file, built-in fallback.
        // This keeps the project configurable without changing code.
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

        // Cleans Gemini output so it can be parsed as JSON even if the model wraps it in markdown fences.
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

        // Converts a relative prompt-file setting into a real path under the web project's content root.
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
