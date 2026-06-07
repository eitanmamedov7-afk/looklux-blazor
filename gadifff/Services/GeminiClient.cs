
// SEARCH INDEX
// AI, GEMINI, HTTP, API, IMAGE, PROMPT, JSON, CONFIG, VALIDATE
//
// Topic: GEMINI HTTP CLIENT
// Purpose: Sends text/image prompts to Gemini and returns model text used by analysis and matching.
// Search keywords: AI GEMINI HTTP API IMAGE PROMPT JSON CONFIG VALIDATE
// When to use it: Show this when explaining how the project communicates with an external AI service.
// Important notes: This class does not decide garment fields or outfit scores; it only sends requests and returns responses.

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

namespace gadifff.Services
{
    // SECTION: GEMINI API CLIENT
    // Topic: GeminiClient class
    // Purpose: Wraps the raw Gemini HTTP API behind one reusable service.
    // Search keywords: GEMINI API HTTP PROMPT IMAGE
    // When to use it: Use when explaining external API integration.
    // Important notes: Called by GarmentFeatureService and MatchingService.
    // Small wrapper around the Gemini HTTP API.
    // Project process affected: garment image analysis and outfit recommendation scoring.
    public class GeminiClient
    {
        // Default model used when config is missing or an older configured model is no longer available.
        private const string DefaultModel = "gemini-2.5-flash";

        private readonly HttpClient _http;
        private readonly ILogger<GeminiClient> _logger;
        private readonly string? _apiKey;
        private readonly string _model;

        // Reads API settings from environment variables first, then from app configuration.
        public GeminiClient(HttpClient http, ILogger<GeminiClient> logger, IConfiguration config)
        {
            _http = http;
            _logger = logger;
            var envKey = ResolveApiKey();
            _apiKey = !string.IsNullOrWhiteSpace(envKey) ? envKey : config["Gemini:ApiKey"];

            _model = NormalizeModel(config["Gemini:Model"]);
        }

        // Lets callers fail early or show a clear setup error when no Gemini key exists.
        public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

        // Sends a prompt, optionally with image bytes, and returns the model text response.
        // Used by GarmentFeatureService for image JSON and MatchingService for outfit scoring JSON.
        public async Task<string> GenerateAsync(string prompt, byte[]? imageBytes = null, string? mimeType = null, CancellationToken ct = default)
        {
            if (!IsConfigured)
            {
                _logger.LogWarning("Gemini API key not configured. Using fallback stub response.");
                return "{}";
            }

            try
            {
                var modelToUse = _model;
                var (statusCode, isSuccess, body) = await SendRequestAsync(modelToUse, prompt, imageBytes, mimeType, ct);

                if (statusCode == HttpStatusCode.NotFound &&
                    !string.Equals(modelToUse, DefaultModel, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "Gemini model {Model} was not found. Retrying with fallback model {FallbackModel}. Set Gemini:Model to update.",
                        modelToUse,
                        DefaultModel);

                    modelToUse = DefaultModel;
                    (statusCode, isSuccess, body) = await SendRequestAsync(modelToUse, prompt, imageBytes, mimeType, ct);
                }

                if (!isSuccess)
                {
                    _logger.LogError("Gemini request failed: {Status} {Body}", statusCode, Shorten(body));
                    throw new InvalidOperationException("Gemini call failed");
                }

                var root = JsonSerializer.Deserialize<JsonElement>(body);
                if (root.TryGetProperty("candidates", out var candidates)
                    && candidates.ValueKind == JsonValueKind.Array
                    && candidates.GetArrayLength() > 0)
                {
                    foreach (var candidate in candidates.EnumerateArray())
                    {
                        if (!candidate.TryGetProperty("content", out var content) ||
                            !content.TryGetProperty("parts", out var parts) ||
                            parts.ValueKind != JsonValueKind.Array)
                        {
                            continue;
                        }

                        foreach (var part in parts.EnumerateArray())
                        {
                            if (part.ValueKind != JsonValueKind.Object)
                                continue;

                            if (part.TryGetProperty("text", out var textElement) &&
                                textElement.ValueKind == JsonValueKind.String)
                            {
                                var text = textElement.GetString();
                                if (!string.IsNullOrWhiteSpace(text))
                                    return text;
                            }
                        }
                    }
                }

                if (root.TryGetProperty("error", out var error) &&
                    error.ValueKind == JsonValueKind.Object &&
                    error.TryGetProperty("message", out var messageElement) &&
                    messageElement.ValueKind == JsonValueKind.String)
                {
                    var errorMessage = messageElement.GetString();
                    if (!string.IsNullOrWhiteSpace(errorMessage))
                    {
                        _logger.LogWarning("Gemini returned error payload: {Message}", Shorten(errorMessage));
                        return errorMessage;
                    }
                }

                _logger.LogWarning("Gemini response missing expected fields. Body: {Body}", Shorten(body));
                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini request error");
                return "";
            }
        }

        // Builds and sends the raw Gemini REST request.
        // Keeping this separate makes retry/fallback model logic easier to read.
        private async Task<(HttpStatusCode StatusCode, bool IsSuccess, string Body)> SendRequestAsync(string model, string prompt, byte[]? imageBytes, string? mimeType, CancellationToken ct)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";
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

            var json = JsonSerializer.Serialize(payload);
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            req.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };

            using var resp = await _http.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            return (resp.StatusCode, resp.IsSuccessStatusCode, body);
        }

        // Creates Gemini request parts: always text, plus inline image data when an upload is being analyzed.
        private static object[] BuildParts(string prompt, byte[]? imageBytes, string? mimeType)
        {
            var parts = new List<object>
            {
                new { text = prompt }
            };

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

            return parts.ToArray();
        }

        // Normalizes model config so old or fully-qualified model names still work.
        private static string NormalizeModel(string? configuredModel)
        {
            if (string.IsNullOrWhiteSpace(configuredModel))
                return DefaultModel;

            var model = configuredModel.Trim();
            if (model.StartsWith("models/", StringComparison.OrdinalIgnoreCase))
                model = model["models/".Length..];

            if (string.Equals(model, "gemini-1.5-flash", StringComparison.OrdinalIgnoreCase))
                return DefaultModel;

            return model;
        }

        // Supports several common environment variable names for the Gemini API key.
        private static string? ResolveApiKey()
        {
            var names = new[]
            {
                "GEMINI_API_KEY",
                "GOOGLE_API_KEY",
                "GEMINI_KEY",
                "GENAI_API_KEY"
            };

            return names
                .Select(Environment.GetEnvironmentVariable)
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        }

        // Keeps logs readable by trimming large API responses.
        private static string Shorten(string input, int max = 400)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Length <= max ? input : input[..max] + "...";
        }
    }
}
