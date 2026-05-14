// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DBL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;

namespace gadifff.Services
{
    public class MatchingService
    {
        public const string ScoringPromptConfigKey = "Matching:ScoringPrompt";
        public const string ScoringPromptFileConfigKey = "Matching:ScoringPromptFile";

        private const string DefaultScoringPromptFile = "Prompts/outfit-scoring.txt";

        private static readonly string BuiltInScoringPrompt = """
You are a fashion stylist.
Use the input combinations and decide the best outfit recommendations.
Think carefully and logically, but return ONLY JSON (no markdown).
Input includes optional free-text user_request. Use it exactly as written.
Output must be either:
1) {
  "results": [
    {
      "index": 0,
      "score": 0,
      "style_label": "short style label",
      "explanation": "1-3 sentences describing why the pieces work together",
      "recommended_places": ["place or occasion", "place or occasion"]
    }
  ]
}
2) {
  "no_match_message": "short user-facing message"
}
Rules:
- If user_request is empty: return the best 1-3 recommendations from ALL combinations.
- If user_request is provided: treat it as a preference and return the best 1-3 combinations that still make sense.
- For non-empty user_request, prioritize request alignment but still return strong outfits if strict matches are weak.
- If request asks for a dominant color (example: "black outfit"), prioritize outfits where all pieces follow that color. Relax only if no such outfit exists.
- For sporty requests, prefer sport/athletic brands over luxury or elegant brands when alternatives exist.
- Prefer returning recommendations whenever possible; use no_match_message only when combinations are empty or completely unusable.
- Do not return duplicate combinations.
- results must be sorted best-to-worst.
- score must be integer 0-100.
- style_label should be concise and realistic (e.g., sporty casual, smart casual, minimal street).
- recommended_places must be an array with 1-4 short strings.
""";

        private readonly GarmentDB _garmentDb;
        private readonly OutfitDB _outfitDb;
        private readonly OutfitGarmentDB _outfitGarmentDb;
        private readonly UserMatchingStateDB _stateDb;
        private readonly GeminiClient _gemini;
        private readonly ILogger<MatchingService> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IConfiguration _config;

        private const int DefaultMinPerType = 1;
        private const int FailureMinPerType = 1;
        private const int ScoreThreshold = 70;
        private static readonly Dictionary<string, string> CanonicalTagByAlias = new(StringComparer.OrdinalIgnoreCase)
        {
            ["office"] = "formal",
            ["business"] = "formal",
            ["smart"] = "formal",
            ["elegant"] = "formal",
            ["dressy"] = "formal",
            ["wedding"] = "formal",
            ["work"] = "office",
            ["professional"] = "office",
            ["casual"] = "casual",
            ["everyday"] = "casual",
            ["daily"] = "casual",
            ["street"] = "streetwear",
            ["urban"] = "streetwear",
            ["sport"] = "sporty",
            ["sports"] = "sporty",
            ["athletic"] = "sporty",
            ["gym"] = "sporty",
            ["workout"] = "sporty",
            ["party"] = "party",
            ["club"] = "party",
            ["date"] = "date-night",
            ["night"] = "date-night",
            ["romantic"] = "date-night",
            ["travel"] = "travel",
            ["vacation"] = "travel",
            ["beach"] = "beach",
            ["rain"] = "rainy",
            ["rainy"] = "rainy",
            ["winter"] = "winter",
            ["summer"] = "summer",
            ["spring"] = "spring",
            ["autumn"] = "fall",
            ["fall"] = "fall"
        };
        private static readonly HashSet<string> BlackColorKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "black", "jet", "charcoal", "graphite", "coal", "onyx", "ink", "ebony"
        };
        private static readonly HashSet<string> SportBrandKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "nike", "adidas", "puma", "reebok", "asics", "new balance", "under armour", "underarmour",
            "fila", "converse", "hoka", "brooks", "mizuno", "salomon", "saucony", "jordan", "champion", "umbro"
        };
        private static readonly HashSet<string> LuxuryBrandKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "gucci", "prada", "armani", "balenciaga", "burberry", "dior", "louis vuitton", "hermes",
            "versace", "valentino", "ysl", "saint laurent", "dolce gabbana", "chanel", "fendi", "bottega"
        };

        public MatchingService(
            GarmentDB garmentDb,
            OutfitDB outfitDb,
            OutfitGarmentDB outfitGarmentDb,
            UserMatchingStateDB stateDb,
            GeminiClient gemini,
            ILogger<MatchingService> logger,
            IHostEnvironment hostEnvironment,
            IConfiguration config)
        {
            _garmentDb = garmentDb;
            _outfitDb = outfitDb;
            _outfitGarmentDb = outfitGarmentDb;
            _stateDb = stateDb;
            _gemini = gemini;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
            _config = config;
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<UserMatchingState> GetOrInitStateAsync(string userId)
        {
            var state = await _stateDb.GetByUserAsync(userId);
            if (state == null)
            {
                state = new UserMatchingState
                {
                    UserId = userId,
                    MinPerType = DefaultMinPerType,
                    LockedAfterFailure = false
                };
                await _stateDb.UpsertAsync(state);
            }

            return state;
        }

        // הסבר: פונקציית בדיקה. מחזירה תשובה לוגית/ניסיון פריסה כדי להחליט על המשך הזרימה.
        public async Task<MatchingGateResult> CanMatchAsync(string userId, IEnumerable<Garment>? garmentsOverride = null)
        {
            var garments = garmentsOverride?.ToList() ?? await _garmentDb.GetByUserAsync(userId);
            var counts = BuildTypeCounts(garments);
            var state = await GetOrInitStateAsync(userId);

            bool HasEnough(string type) =>
                counts.TryGetValue(type, out var c) && c >= state.MinPerType;

            var ok = HasEnough("shirt") && HasEnough("pants") && HasEnough("shoes");
            return new MatchingGateResult
            {
                Allowed = ok,
                MinPerTypeRequired = state.MinPerType,
                Counts = counts
            };
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public async Task RecordFailureAsync(string userId)
        {
            var state = await GetOrInitStateAsync(userId);
            state.LockedAfterFailure = true;
            state.MinPerType = FailureMinPerType;
            state.LastFailureAt = DateTime.UtcNow;
            await _stateDb.UpsertAsync(state);
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public async Task RecordSuccessAsync(string userId)
        {
            var state = await GetOrInitStateAsync(userId);
            state.LockedAfterFailure = false;
            state.MinPerType = DefaultMinPerType;
            state.LastSuccessAt = DateTime.UtcNow;
            await _stateDb.UpsertAsync(state);
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public async Task<MatchResult> FindMatchesAsync(
            string userId,
            string[] seedGarmentIds,
            IEnumerable<Garment>? cachedGarments = null,
            bool allowNoSeed = false,
            string? userRequest = null)
        {
            var cachedList = cachedGarments?.ToList() ?? new List<Garment>();
            var authoritativeList = await _garmentDb.GetByUserAsync(userId);
            var garmentList = authoritativeList.Count > 0
                ? MergeGarments(cachedList, authoritativeList)
                : cachedList;

            var gate = await CanMatchAsync(userId, garmentList);
            if (!gate.Allowed)
                return MatchResult.CreateBlocked(gate.MinPerTypeRequired, gate.Counts);

            var requestedSeedIds = seedGarmentIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var seedLookupSource = authoritativeList.Count > 0 ? authoritativeList : garmentList;
            var seedLookup = BuildGarmentLookup(seedLookupSource);
            var seed = new List<Garment>();
            var missingSeedIds = new List<string>();

            foreach (var id in requestedSeedIds)
            {
                if (seedLookup.TryGetValue(id, out var g))
                    seed.Add(g);
                else
                    missingSeedIds.Add(id);
            }

            if (_hostEnvironment.IsDevelopment())
            {
                _logger.LogInformation(
                    "Matcher seed resolve user={UserId} requested=[{Requested}] resolved=[{Resolved}] missing=[{Missing}] cachedCount={CachedCount} authoritativeCount={AuthoritativeCount}",
                    userId,
                    string.Join(",", requestedSeedIds),
                    string.Join(",", seed.Select(s => $"{s.GarmentId}:{NormalizeTypeName(s.Type)}")),
                    string.Join(",", missingSeedIds),
                    cachedList.Count,
                    authoritativeList.Count);
            }

            if (seed.Count == 0 && !allowNoSeed)
                return MatchResult.CreateError("No valid garments selected.");

            var seedType = seed.Count > 0 ? DetermineSeedType(seed) : "none";

            var neededTypes = new[] { "shirt", "pants", "shoes" };
            var pools = new Dictionary<string, List<Garment>>(StringComparer.OrdinalIgnoreCase);
            var useCachedData = cachedGarments != null;
            foreach (var t in neededTypes)
            {
                var exclude = seed.Where(s => IsType(s, t))
                                  .Select(s => s.GarmentId)
                                  .ToList();

                if (useCachedData)
                {
                    pools[t] = garmentList
                        .Where(g => IsType(g, t) &&
                                    !exclude.Contains(g.GarmentId, StringComparer.OrdinalIgnoreCase))
                        .ToList();
                }
                else
                {
                    pools[t] = await _garmentDb.GetByUserAndTypeAsync(userId, t, exclude);
                }
            }

            var combinations = BuildCombinations(seed, pools);
            if (combinations.Count == 0)
            {
                await RecordFailureAsync(userId);
                return MatchResult.CreateNoMatches("No matches would look good right now. Add more garments and try again.");
            }

            var hasPromptRequest = !string.IsNullOrWhiteSpace(userRequest);
            var requestIntent = ParseRequestIntent(userRequest);

            var scoring = await ScoreCombinationsAsync(combinations, seedType, requestedSeedIds, userRequest);
            if (!string.IsNullOrWhiteSpace(scoring.ErrorMessage))
            {
                _logger.LogWarning("Gemini matcher scoring returned an error payload. Falling back to deterministic local scores. Error: {Error}", scoring.ErrorMessage);
                scoring = ScoringOutcome.Empty;
            }

            var noMatchMessage = scoring.NoMatchMessage;
            var comboLookup = BuildCombinationLookup(combinations);

            var scored = scoring.Results
                .Where(IsRecommendationComplete)
                .Select(x => ResolveRecommendation(x, comboLookup, requestIntent))
                .Where(x => x != null)
                .Select(x => x!)
                .GroupBy(x => GetRecommendationSignature(x.Recommendation), StringComparer.OrdinalIgnoreCase)
                .Select(group => group
                    .OrderByDescending(x => x.Recommendation.Score)
                    .ThenBy(x => x.Recommendation.Rank)
                    .First()
                    .Recommendation)
                .OrderByDescending(x => ComputePromptPriority(x, comboLookup, requestIntent))
                .ThenByDescending(x => x.Score)
                .ThenBy(x => x.Rank)
                .ToList();

            var maxResults = Math.Min(3, combinations.Count);
            if (scored.Count < maxResults)
            {
                var localCandidates = combinations;

                var used = scored
                    .Select(GetRecommendationSignature)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var fallback = localCandidates
                    .Select((combo, idx) => new
                    {
                        Combo = combo,
                        Recommendation = BuildFallbackRecommendation(combo, idx + 1, requestIntent),
                        PromptPriority = ComputePromptFitScore(combo, requestIntent)
                    })
                    .Where(x => !used.Contains(GetRecommendationSignature(x.Recommendation)))
                    .Where(x => IsRecommendationViable(x.Recommendation, x.PromptPriority, hasPromptRequest))
                    .OrderByDescending(x => x.PromptPriority)
                    .ThenByDescending(x => x.Recommendation.Score)
                    .ThenBy(x => x.Recommendation.Rank)
                    .Take(maxResults - scored.Count)
                    .Select(x => x.Recommendation);

                scored.AddRange(fallback);
                scored = scored
                    .GroupBy(GetRecommendationSignature, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group
                        .OrderByDescending(x => x.Score)
                        .ThenBy(x => x.Rank)
                        .First())
                    .OrderByDescending(x => ComputePromptPriority(x, comboLookup, requestIntent))
                    .ThenByDescending(x => x.Score)
                    .ThenBy(x => x.Rank)
                    .ToList();
            }

            var top = scored
                .Where(x => IsRecommendationViable(x, ComputePromptPriority(x, comboLookup, requestIntent), hasPromptRequest))
                .Take(maxResults)
                .ToList();

            if (top.Count == 0 && scored.Count > 0)
                top.Add(scored[0]);

            top = EnsureDescendingDistinctScores(top)
                .Select((x, idx) => x with { Rank = idx + 1 })
                .ToList();

            if (!top.Any())
            {
                await RecordFailureAsync(userId);
                var fallbackMessage = !string.IsNullOrWhiteSpace(noMatchMessage)
                    ? noMatchMessage
                    : (hasPromptRequest && requestIntent.Tokens.Count > 0
                        ? "No combinations match your request yet. Try a broader prompt."
                        : "No matches would look good right now. Add more garments and try again.");
                return MatchResult.CreateNoMatches(fallbackMessage);
            }

            await RecordSuccessAsync(userId);
            return MatchResult.CreateSuccess(top);
        }

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        public async Task<SaveOutfitResult> SaveOutfitSuggestionAsync(
            string userId,
            ScoredCombination recommendation,
            IEnumerable<string>? requestedSeedGarmentIds = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return SaveOutfitResult.Fail("Missing user id.");

            if (string.IsNullOrWhiteSpace(recommendation.ShirtId) ||
                string.IsNullOrWhiteSpace(recommendation.PantsId) ||
                string.IsNullOrWhiteSpace(recommendation.ShoesId))
            {
                return SaveOutfitResult.Fail("Suggestion is incomplete.");
            }

            var requestedIds = (requestedSeedGarmentIds ?? Enumerable.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var garments = await _garmentDb.GetByUserAsync(userId);
            var garmentLookup = BuildGarmentLookup(garments);
            var resolvedSeed = requestedIds
                .Where(garmentLookup.ContainsKey)
                .Select(id => garmentLookup[id])
                .ToList();

            var seedType = resolvedSeed.Count > 0 ? DetermineSeedType(resolvedSeed) : "unknown";
            var seedGarmentId = resolvedSeed.Select(x => x.GarmentId).FirstOrDefault();
            var requestedGarmentIds = resolvedSeed.Count > 0
                ? string.Join(",", resolvedSeed.Select(x => x.GarmentId))
                : (seedGarmentId ?? string.Empty);

            var isDuplicate = await _outfitDb.ExistsDuplicateAsync(
                userId,
                recommendation.ShirtId,
                recommendation.PantsId,
                recommendation.ShoesId,
                seedType,
                seedGarmentId);

            if (isDuplicate)
                return SaveOutfitResult.DuplicateResult("This outfit is already saved.");

            var outfitId = Guid.NewGuid().ToString();
            var outfit = new Outfit
            {
                OutfitId = outfitId,
                UserId = userId,
                ShirtGarmentId = recommendation.ShirtId,
                PantsGarmentId = recommendation.PantsId,
                ShoesGarmentId = recommendation.ShoesId,
                Score = recommendation.Score,
                Rank = recommendation.Rank,
                StyleLabel = recommendation.StyleLabel,
                Explanation = recommendation.Explanation,
                RecommendedPlaces = recommendation.RecommendedPlaces,
                SeedType = seedType,
                SeedGarmentId = seedGarmentId,
                LabelIsCompatible = recommendation.Score >= ScoreThreshold,
                LabelSource = "auto",
                RequestedGarmentIds = string.IsNullOrWhiteSpace(requestedGarmentIds) ? null : requestedGarmentIds,
                CreatedAt = DateTime.UtcNow
            };

            var inserted = await _outfitDb.CreateAsync(outfit);
            if (inserted <= 0)
                return SaveOutfitResult.Fail("Failed to save outfit.");

            var seedSet = new HashSet<string>(
                resolvedSeed.Select(x => x.GarmentId),
                StringComparer.OrdinalIgnoreCase);
            var garmentRows = BuildOutfitGarments(outfitId, recommendation, seedSet);
            var linksSaved = await _outfitGarmentDb.CreateForOutfitAsync(outfitId, garmentRows);
            if (!linksSaved)
                return SaveOutfitResult.Fail("Outfit row saved, but garment links failed.");

            return SaveOutfitResult.SuccessResult(outfitId);
        }

        // הסבר: פונקציית בנייה. מרכיבה אובייקט/בקשה/פלט מתוך נתוני קלט לפני השלב הבא בזרימה.
        private static List<OutfitGarment> BuildOutfitGarments(
            string outfitId,
            ScoredCombination recommendation,
            HashSet<string> seedIdSet)
        {
            var now = DateTime.UtcNow;
            return new List<OutfitGarment>
            {
                new()
                {
                    OutfitGarmentId = Guid.NewGuid().ToString(),
                    OutfitId = outfitId,
                    GarmentId = recommendation.ShirtId,
                    GarmentType = "shirt",
                    IsSeed = seedIdSet.Contains(recommendation.ShirtId),
                    CreatedAt = now
                },
                new()
                {
                    OutfitGarmentId = Guid.NewGuid().ToString(),
                    OutfitId = outfitId,
                    GarmentId = recommendation.PantsId,
                    GarmentType = "pants",
                    IsSeed = seedIdSet.Contains(recommendation.PantsId),
                    CreatedAt = now
                },
                new()
                {
                    OutfitGarmentId = Guid.NewGuid().ToString(),
                    OutfitId = outfitId,
                    GarmentId = recommendation.ShoesId,
                    GarmentType = "shoes",
                    IsSeed = seedIdSet.Contains(recommendation.ShoesId),
                    CreatedAt = now
                }
            };
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static string DetermineSeedType(IEnumerable<Garment> seed)
        {
            var types = seed
                .Select(g => NormalizeTypeName(g.Type))
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (types.Count == 1)
                return types[0];
            if (types.Count > 1)
                return "multi";
            return "unknown";
        }

        // הסבר: פונקציית בנייה. מרכיבה אובייקט/בקשה/פלט מתוך נתוני קלט לפני השלב הבא בזרימה.
        private List<Combination> BuildCombinations(List<Garment> seed, Dictionary<string, List<Garment>> pools)
        {
            Garment? seedShirt = seed.FirstOrDefault(g => IsType(g, "shirt"));
            Garment? seedPants = seed.FirstOrDefault(g => IsType(g, "pants"));
            Garment? seedShoes = seed.FirstOrDefault(g => IsType(g, "shoes"));

            var shirts = seedShirt != null ? new List<Garment> { seedShirt } : pools["shirt"];
            var pants = seedPants != null ? new List<Garment> { seedPants } : pools["pants"];
            var shoes = seedShoes != null ? new List<Garment> { seedShoes } : pools["shoes"];

            var list = new List<Combination>();
            foreach (var sh in shirts)
            foreach (var pa in pants)
            foreach (var so in shoes)
            {
                list.Add(new Combination
                {
                    Shirt = sh,
                    Pants = pa,
                    Shoes = so
                });
            }

            return list;
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private async Task<ScoringOutcome> ScoreCombinationsAsync(
            List<Combination> combos,
            string seedType,
            List<string> seedIds,
            string? userRequest)
        {
            var hasPromptRequest = !string.IsNullOrWhiteSpace(userRequest);
            if (!_gemini.IsConfigured)
            {
                _logger.LogWarning("Gemini is not configured. Falling back to deterministic local matcher scores.");
                return ScoringOutcome.Empty;
            }

            try
            {
                var scoringInstructions = await LoadScoringPromptAsync();
                var prompt = BuildMatchPrompt(combos, seedType, seedIds, userRequest, scoringInstructions);
                var text = await _gemini.GenerateAsync(prompt);
                if (string.IsNullOrWhiteSpace(text))
                    return ScoringOutcome.Empty;

                var parsed = ParseScoringOutcome(text, combos);
                if (!string.IsNullOrWhiteSpace(parsed.ErrorMessage))
                    return parsed;
                if (!string.IsNullOrWhiteSpace(parsed.NoMatchMessage) || parsed.Results.Count > 0)
                    return parsed;

                if (hasPromptRequest)
                {
                    var apiMessage = NormalizeApiMessage(text);
                    if (LooksLikeNoMatchMessage(apiMessage))
                        return ScoringOutcome.WithNoMatch(apiMessage);
                }

                return ScoringOutcome.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini scoring failed.");
                return ScoringOutcome.Empty;
            }
        }

        // הסבר: פונקציית טעינה. טוענת נתונים/קונפיג/משאבים לפני שימוש בשלבים הבאים.
        private async Task<string> LoadScoringPromptAsync()
        {
            var configPrompt = _config[ScoringPromptConfigKey];
            if (!string.IsNullOrWhiteSpace(configPrompt))
                return configPrompt;

            var configuredPath = _config[ScoringPromptFileConfigKey];
            var path = ResolvePromptPath(configuredPath);
            if (File.Exists(path))
            {
                var fromFile = await File.ReadAllTextAsync(path);
                if (!string.IsNullOrWhiteSpace(fromFile))
                    return fromFile;
            }

            return BuiltInScoringPrompt;
        }

        // הסבר: פונקציית resolve. מחליטה מה הערך/המקור הנכון לשימוש לפי סדר עדיפויות ברור.
        private string ResolvePromptPath(string? configuredPath)
        {
            var value = string.IsNullOrWhiteSpace(configuredPath)
                ? DefaultScoringPromptFile
                : configuredPath.Trim();

            if (Path.IsPathRooted(value))
                return value;

            return Path.Combine(_hostEnvironment.ContentRootPath, value.Replace('/', Path.DirectorySeparatorChar));
        }

        // הסבר: פונקציית בנייה. מרכיבה אובייקט/בקשה/פלט מתוך נתוני קלט לפני השלב הבא בזרימה.
        private static string BuildMatchPrompt(
            List<Combination> combos,
            string seedType,
            List<string> seedIds,
            string? userRequest,
            string scoringInstructions)
        {
            var payload = new
            {
                seed_type = seedType,
                seed_ids = seedIds,
                user_request = userRequest ?? string.Empty,
                combinations = combos.Select((combo, index) => new
                {
                    index,
                    shirt = BuildGarmentPromptObject(combo.Shirt),
                    pants = BuildGarmentPromptObject(combo.Pants),
                    shoes = BuildGarmentPromptObject(combo.Shoes)
                })
            };

            var json = JsonSerializer.Serialize(payload);
            return $"{scoringInstructions}\nInput JSON:\n{json}";
        }

        // הסבר: פונקציית בנייה. מרכיבה אובייקט/בקשה/פלט מתוך נתוני קלט לפני השלב הבא בזרימה.
        private static object BuildGarmentPromptObject(Garment garment)
        {
            return new
            {
                id = garment.GarmentId,
                type = NormalizeTypeName(garment.Type),
                color = garment.Color,
                color_secondary = garment.ColorSecondary,
                pattern = garment.Pattern,
                style_category = garment.StyleCategory,
                season = garment.Season,
                occasion = garment.Occasion,
                formality_level = garment.FormalityLevel,
                style_tags = ParseStyleTags(garment.StyleTags, garment.FeatureJson),
                fit = garment.Fit,
                material = garment.Material,
                sleeve = garment.Sleeve,
                length = garment.Length,
                brand = garment.Brand
            };
        }

        // הסבר: פונקציית פירסור. ממירה טקסט/JSON/פרמטרים למבנה נתונים שנוח לעבוד איתו.
        private static List<string> ParseStyleTags(string? styleTagsJson, string? featureJson)
        {
            static List<string> ExtractFromJsonArray(string? json)
            {
                if (string.IsNullOrWhiteSpace(json))
                    return new List<string>();

                try
                {
                    var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.ValueKind != JsonValueKind.Array)
                        return new List<string>();

                    return doc.RootElement
                        .EnumerateArray()
                        .Select(x => x.GetString())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x!.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }
                catch
                {
                    return new List<string>();
                }
            }

            var tags = ExtractFromJsonArray(styleTagsJson);
            if (tags.Count > 0)
                return tags;

            if (string.IsNullOrWhiteSpace(featureJson))
                return new List<string>();

            try
            {
                var doc = JsonDocument.Parse(featureJson);
                if (!doc.RootElement.TryGetProperty("style_tags", out var styleTagsElement) ||
                    styleTagsElement.ValueKind != JsonValueKind.Array)
                    return new List<string>();

                return styleTagsElement
                    .EnumerateArray()
                    .Select(x => x.GetString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        // הסבר: פונקציית פירסור. ממירה טקסט/JSON/פרמטרים למבנה נתונים שנוח לעבוד איתו.
        private ScoringOutcome ParseScoringOutcome(string text, List<Combination> combos)
        {
            try
            {
                var raw = NormalizeJson(text);
                if (string.IsNullOrWhiteSpace(raw))
                    return ScoringOutcome.Empty;

                var doc = JsonSerializer.Deserialize<JsonElement>(raw);
                if (doc.ValueKind == JsonValueKind.Object)
                {
                    var noMatchMessage = ReadStringByNames(doc, "no_match_message", "noMatchMessage", "no_match");
                    if (!string.IsNullOrWhiteSpace(noMatchMessage))
                        return ScoringOutcome.WithNoMatch(noMatchMessage);

                    var status = ReadStringByNames(doc, "status", "result_state");
                    if (IsNoMatchStatus(status))
                    {
                        var statusMessage = ReadStringByNames(doc, "message", "reason", "detail", "explanation");
                        return ScoringOutcome.WithNoMatch(string.IsNullOrWhiteSpace(statusMessage)
                            ? "No suitable match found for this request."
                            : statusMessage);
                    }

                    var errorMessage = ReadStringByNames(doc, "error_message", "errorMessage", "error");
                    if (!string.IsNullOrWhiteSpace(errorMessage))
                        return ScoringOutcome.WithError(errorMessage);

                    if (TryGetPropertyByNames(doc, out var results, "results", "recommendations", "matches", "suggestions", "items"))
                    {
                        if (results.ValueKind == JsonValueKind.Array)
                            return ScoringOutcome.WithResults(ParseRecommendationArray(results, combos));

                        if (results.ValueKind == JsonValueKind.Object &&
                            TryParseRecommendationItem(results, combos, 1, out var singleResult))
                        {
                            return ScoringOutcome.WithResults(new List<ScoredCombination> { singleResult });
                        }
                    }

                    if (TryParseRecommendationItem(doc, combos, 1, out var directResult))
                    {
                        return ScoringOutcome.WithResults(new List<ScoredCombination> { directResult });
                    }

                    var fallbackMessage = ReadStringByNames(doc, "message", "reason");
                    if (!string.IsNullOrWhiteSpace(fallbackMessage))
                    {
                        return ScoringOutcome.WithNoMatch(fallbackMessage);
                    }
                }

                if (doc.ValueKind == JsonValueKind.Array)
                {
                    return ScoringOutcome.WithResults(ParseRecommendationArray(doc, combos));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse matcher recommendations. Raw: {Raw}", text);
            }

            return ScoringOutcome.Empty;
        }

        // הסבר: פונקציית פירסור. ממירה טקסט/JSON/פרמטרים למבנה נתונים שנוח לעבוד איתו.
        private static List<ScoredCombination> ParseRecommendationArray(JsonElement array, List<Combination> combos)
        {
            var list = new List<ScoredCombination>();
            foreach (var item in array.EnumerateArray())
            {
                if (TryParseRecommendationItem(item, combos, list.Count + 1, out var scored))
                {
                    list.Add(scored);
                }
            }

            return list;
        }

        // הסבר: פונקציית בדיקה. מחזירה תשובה לוגית/ניסיון פריסה כדי להחליט על המשך הזרימה.
        private static bool TryParseRecommendationItem(
            JsonElement item,
            List<Combination> combos,
            int fallbackRank,
            out ScoredCombination scored)
        {
            scored = default!;
            if (item.ValueKind != JsonValueKind.Object)
                return false;

            Combination? combo = null;
            if (TryReadIntByNames(item, out var index, "index", "idx", "combination_index", "combinationIndex"))
            {
                if (index >= 0 && index < combos.Count)
                    combo = combos[index];
                else if (index >= 1 && index <= combos.Count)
                    combo = combos[index - 1];
            }

            var shirtId = ReadGarmentId(item, "shirt_id", "shirtId", "shirt");
            var pantsId = ReadGarmentId(item, "pants_id", "pantsId", "pants");
            var shoesId = ReadGarmentId(item, "shoes_id", "shoesId", "shoes");

            if (combo != null)
            {
                shirtId ??= combo.Shirt.GarmentId;
                pantsId ??= combo.Pants.GarmentId;
                shoesId ??= combo.Shoes.GarmentId;
            }

            if (string.IsNullOrWhiteSpace(shirtId) ||
                string.IsNullOrWhiteSpace(pantsId) ||
                string.IsNullOrWhiteSpace(shoesId))
            {
                return false;
            }

            var score = 0;
            if (TryReadIntByNames(item, out var parsedScore, "score", "match_score", "matchScore", "rating"))
                score = parsedScore;
            score = Math.Clamp(score, 0, 100);

            var styleLabel = ReadStringByNames(item, "style_label", "styleLabel", "label") ?? "balanced casual";
            var explanation = ReadStringByNames(item, "explanation", "reason", "why")
                ?? "The colors and proportions create a balanced outfit.";
            var recommendedPlaces = ReadStringArrayOrStringByNames(item, "recommended_places", "recommendedPlaces", "places", "occasions")
                ?? "daily";
            var rank = TryReadIntByNames(item, out var parsedRank, "rank", "position") ? parsedRank : fallbackRank;

            scored = new ScoredCombination
            {
                ShirtId = shirtId,
                PantsId = pantsId,
                ShoesId = shoesId,
                Score = score,
                Rank = rank,
                StyleLabel = styleLabel,
                Explanation = explanation,
                RecommendedPlaces = recommendedPlaces
            };
            return true;
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static string? ReadGarmentId(JsonElement obj, string snakeIdKey, string camelIdKey, string objectKey)
        {
            var direct = ReadStringByNames(obj, snakeIdKey, camelIdKey);
            if (!string.IsNullOrWhiteSpace(direct))
                return direct;

            if (!TryGetPropertyByNames(obj, out var nested, objectKey))
                return null;

            if (nested.ValueKind == JsonValueKind.String)
                return nested.GetString();

            if (nested.ValueKind == JsonValueKind.Object)
                return ReadStringByNames(nested, "id", "garment_id", "garmentId");

            return null;
        }

        // הסבר: פונקציית בדיקה. מחזירה תשובה לוגית/ניסיון פריסה כדי להחליט על המשך הזרימה.
        private static bool TryReadInt(JsonElement element, out int value)
        {
            value = 0;
            if (element.ValueKind == JsonValueKind.Number)
            {
                if (element.TryGetInt32(out value))
                    return true;

                if (element.TryGetDouble(out var asDouble))
                {
                    value = (int)Math.Round(asDouble);
                    return true;
                }

                return false;
            }

            if (element.ValueKind == JsonValueKind.String)
                return int.TryParse(element.GetString(), out value);
            return false;
        }

        // הסבר: פונקציית בדיקה. מחזירה תשובה לוגית/ניסיון פריסה כדי להחליט על המשך הזרימה.
        private static bool TryReadIntByNames(JsonElement obj, out int value, params string[] names)
        {
            value = 0;
            foreach (var name in names)
            {
                if (TryGetPropertyByNames(obj, out var prop, name) && TryReadInt(prop, out value))
                    return true;
            }

            return false;
        }

        // הסבר: פונקציית בדיקה. מחזירה תשובה לוגית/ניסיון פריסה כדי להחליט על המשך הזרימה.
        private static bool TryGetPropertyByNames(JsonElement obj, out JsonElement value, params string[] names)
        {
            foreach (var property in obj.EnumerateObject())
            {
                foreach (var name in names)
                {
                    if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        value = property.Value;
                        return true;
                    }
                }
            }

            value = default;
            return false;
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static string? ReadString(JsonElement obj, string name)
        {
            return obj.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static string? ReadStringByNames(JsonElement obj, params string[] names)
        {
            if (!TryGetPropertyByNames(obj, out var value, names))
                return null;

            return value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static string? ReadStringArrayOrString(JsonElement obj, string name)
        {
            if (!obj.TryGetProperty(name, out var value))
                return null;

            if (value.ValueKind == JsonValueKind.String)
                return value.GetString();

            if (value.ValueKind == JsonValueKind.Array)
            {
                var list = value.EnumerateArray()
                    .Where(x => x.ValueKind == JsonValueKind.String)
                    .Select(x => x.GetString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!.Trim())
                    .ToList();

                return list.Count == 0 ? null : string.Join(", ", list);
            }

            return null;
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static string? ReadStringArrayOrStringByNames(JsonElement obj, params string[] names)
        {
            foreach (var name in names)
            {
                var value = ReadStringArrayOrString(obj, name);
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return null;
        }

        // הסבר: פונקציית נרמול. מנקה ומאחד פורמט נתונים כדי למנוע חוסר עקביות בהמשך הזרימה.
        private static string NormalizeJson(string? raw)
        {
            var value = (raw ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            if (TryParseJson(value))
                return value;

            if (value.StartsWith("```", StringComparison.Ordinal))
            {
                value = value.Trim('`').Trim();
                if (value.StartsWith("json", StringComparison.OrdinalIgnoreCase))
                    value = value[4..].Trim();

                if (TryParseJson(value))
                    return value;
            }

            var objectStart = value.IndexOf('{');
            var arrayStart = value.IndexOf('[');
            if (objectStart >= 0 && (arrayStart < 0 || objectStart <= arrayStart))
            {
                var objectEnd = value.LastIndexOf('}');
                if (objectEnd > objectStart)
                {
                    var candidate = value[objectStart..(objectEnd + 1)];
                    if (TryParseJson(candidate))
                        return candidate;
                }
            }

            if (arrayStart >= 0)
            {
                var arrayEnd = value.LastIndexOf(']');
                if (arrayEnd > arrayStart)
                {
                    var candidate = value[arrayStart..(arrayEnd + 1)];
                    if (TryParseJson(candidate))
                        return candidate;
                }
            }

            if (objectStart >= 0)
            {
                var objectEnd = value.LastIndexOf('}');
                if (objectEnd > objectStart)
                    return value[objectStart..(objectEnd + 1)];
            }

            if (arrayStart >= 0)
            {
                var arrayEnd = value.LastIndexOf(']');
                if (arrayEnd > arrayStart)
                    return value[arrayStart..(arrayEnd + 1)];
            }

            return string.Empty;
        }

        // הסבר: פונקציית בדיקה. מחזירה תשובה לוגית/ניסיון פריסה כדי להחליט על המשך הזרימה.
        private static bool IsNoMatchStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;

            var normalized = status.Trim().ToLowerInvariant();
            return normalized is "no_match" or "nomatch" or "no-match" or "none" or "unmatched";
        }

        // הסבר: פונקציית בדיקה. מחזירה תשובה לוגית/ניסיון פריסה כדי להחליט על המשך הזרימה.
        private static bool TryParseJson(string input)
        {
            try
            {
                JsonSerializer.Deserialize<JsonElement>(input);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // הסבר: פונקציית נרמול. מנקה ומאחד פורמט נתונים כדי למנוע חוסר עקביות בהמשך הזרימה.
        private static string NormalizeApiMessage(string? raw)
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

            if (value.Length > 280)
                return value[..280].TrimEnd() + "...";

            return value;
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static bool LooksLikeNoMatchMessage(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return false;

            var text = message.Trim().ToLowerInvariant();
            var signals = new[]
            {
                "no match",
                "no suitable",
                "cannot satisfy",
                "can't satisfy",
                "not enough",
                "no outfit",
                "could not find",
                "nonsense",
                "unclear request"
            };

            return signals.Any(text.Contains);
        }

        // הסבר: פונקציית בדיקה. מחזירה תשובה לוגית/ניסיון פריסה כדי להחליט על המשך הזרימה.
        private static bool IsRecommendationComplete(ScoredCombination recommendation) =>
            !string.IsNullOrWhiteSpace(recommendation.ShirtId) &&
            !string.IsNullOrWhiteSpace(recommendation.PantsId) &&
            !string.IsNullOrWhiteSpace(recommendation.ShoesId);

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        private static string GetRecommendationSignature(ScoredCombination recommendation) =>
            $"{recommendation.ShirtId}|{recommendation.PantsId}|{recommendation.ShoesId}".ToLowerInvariant();

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        private static string GetCombinationSignature(Combination combination) =>
            $"{combination.Shirt.GarmentId}|{combination.Pants.GarmentId}|{combination.Shoes.GarmentId}".ToLowerInvariant();

        // הסבר: פונקציית בנייה. מרכיבה אובייקט/בקשה/פלט מתוך נתוני קלט לפני השלב הבא בזרימה.
        private static Dictionary<string, Combination> BuildCombinationLookup(IEnumerable<Combination> combinations)
        {
            var lookup = new Dictionary<string, Combination>(StringComparer.OrdinalIgnoreCase);
            foreach (var combination in combinations)
            {
                var signature = GetCombinationSignature(combination);
                if (!string.IsNullOrWhiteSpace(signature))
                    lookup[signature] = combination;
            }

            return lookup;
        }

        // הסבר: פונקציית resolve. מחליטה מה הערך/המקור הנכון לשימוש לפי סדר עדיפויות ברור.
        private static ResolvedRecommendation? ResolveRecommendation(
            ScoredCombination recommendation,
            IReadOnlyDictionary<string, Combination> combinationLookup,
            RequestIntent requestIntent)
        {
            var signature = GetRecommendationSignature(recommendation);
            if (!combinationLookup.TryGetValue(signature, out var combo))
                return null;

            var fallbackRank = recommendation.Rank > 0 ? recommendation.Rank : 1;
            var fallback = BuildFallbackRecommendation(combo, fallbackRank, requestIntent);
            var modelWeight = requestIntent.Tokens.Count > 0 ? 0.52d : 0.75d;
            var blendedScore = (int)Math.Round((recommendation.Score * modelWeight) + (fallback.Score * (1d - modelWeight)));

            return new ResolvedRecommendation(recommendation with
            {
                Score = Math.Clamp(blendedScore, 0, 100)
            }, combo);
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static int ComputePromptPriority(
            ScoredCombination recommendation,
            IReadOnlyDictionary<string, Combination> combinationLookup,
            RequestIntent requestIntent)
        {
            if (requestIntent.Tokens.Count == 0)
                return 0;

            var signature = GetRecommendationSignature(recommendation);
            if (!combinationLookup.TryGetValue(signature, out var combo))
                return 0;

            return ComputePromptFitScore(combo, requestIntent);
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static int ComputePromptFitScore(Combination combo, RequestIntent requestIntent)
        {
            if (requestIntent.Tokens.Count == 0)
                return 0;

            var tags = BuildComboTags(combo);
            var matched = requestIntent.Tokens.Count(tags.Contains);
            var ratio = matched / (double)requestIntent.Tokens.Count;
            var score = (int)Math.Round((ratio * 60d) - 12d);

            if (requestIntent.WantsBlackDominant)
            {
                var blackPieces = CountBlackPieces(combo);
                score += blackPieces switch
                {
                    3 => 45,
                    2 => 12,
                    1 => -18,
                    _ => -36
                };
            }

            if (requestIntent.WantsSporty)
            {
                var sportyBrands = CountSportBrandPieces(combo);
                var luxuryBrands = CountLuxuryBrandPieces(combo);
                if (sportyBrands > 0)
                    score += 8 + (sportyBrands * 6);
                if (luxuryBrands > 0 && sportyBrands == 0)
                    score -= 18 + (luxuryBrands * 6);
                else if (luxuryBrands > 0)
                    score -= luxuryBrands * 7;
            }

            return Math.Clamp(score, -80, 95);
        }

        // הסבר: פונקציית בדיקה. מחזירה תשובה לוגית/ניסיון פריסה כדי להחליט על המשך הזרימה.
        private static bool IsRecommendationViable(
            ScoredCombination recommendation,
            int promptPriority,
            bool hasPromptRequest)
        {
            if (!hasPromptRequest)
                return recommendation.Score >= 40;

            return recommendation.Score >= 35 || promptPriority >= 24;
        }

        // הסבר: פונקציית בדיקה. מחזירה תשובה לוגית/ניסיון פריסה כדי להחליט על המשך הזרימה.
        private static bool IsPromptMatch(Combination combo, RequestIntent requestIntent)
        {
            if (requestIntent.Tokens.Count == 0)
                return true;

            var tags = BuildComboTags(combo);
            if (tags.Count == 0)
                return false;

            var matched = requestIntent.Tokens.Count(tags.Contains);
            if (matched == 0)
                return false;
            return true;
        }

        // הסבר: פונקציית בנייה. מרכיבה אובייקט/בקשה/פלט מתוך נתוני קלט לפני השלב הבא בזרימה.
        private static ScoredCombination BuildFallbackRecommendation(Combination combo, int rank, RequestIntent requestIntent)
        {
            var shirtLevel = combo.Shirt.FormalityLevel ?? 3;
            var pantsLevel = combo.Pants.FormalityLevel ?? 3;
            var shoesLevel = combo.Shoes.FormalityLevel ?? 3;

            var formalityDelta = Math.Abs(shirtLevel - pantsLevel) + Math.Abs(pantsLevel - shoesLevel) + Math.Abs(shirtLevel - shoesLevel);
            var formalityScore = 100 - (formalityDelta * 8);

            var styles = new[] { combo.Shirt.StyleCategory, combo.Pants.StyleCategory, combo.Shoes.StyleCategory }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim().ToLowerInvariant())
                .ToList();
            var styleScore = styles.Count == 0
                ? 50
                : styles.GroupBy(x => x).OrderByDescending(g => g.Count()).First().Count() switch
                {
                    3 => 100,
                    2 => 75,
                    _ => 55
                };

            var seasons = new[] { combo.Shirt.Season, combo.Pants.Season, combo.Shoes.Season }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim().ToLowerInvariant())
                .ToList();
            var seasonScore = seasons.Count <= 1
                ? 65
                : seasons.GroupBy(x => x).OrderByDescending(g => g.Count()).First().Count() switch
                {
                    3 => 100,
                    2 => 75,
                    _ => 55
                };

            var occasions = new[] { combo.Shirt.Occasion, combo.Pants.Occasion, combo.Shoes.Occasion }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim().ToLowerInvariant())
                .ToList();
            var occasionScore = occasions.Count <= 1
                ? 65
                : occasions.GroupBy(x => x).OrderByDescending(g => g.Count()).First().Count() switch
                {
                    3 => 100,
                    2 => 75,
                    _ => 55
                };

            var colors = new[] { combo.Shirt.Color, combo.Pants.Color, combo.Shoes.Color }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim().ToLowerInvariant())
                .ToList();
            var distinctColorCount = colors.Distinct(StringComparer.OrdinalIgnoreCase).Count();
            var colorScore = distinctColorCount switch
            {
                0 => 55,
                1 => 72,
                2 => 84,
                3 => 78,
                _ => 70
            };

            var baseScore = (int)Math.Round(
                (formalityScore * 0.35) +
                (styleScore * 0.25) +
                (occasionScore * 0.15) +
                (seasonScore * 0.10) +
                (colorScore * 0.15));

            var deterministicJitter = Math.Abs(HashCode.Combine(combo.Shirt.GarmentId, combo.Pants.GarmentId, combo.Shoes.GarmentId)) % 5;
            var requestBias = ComputeRequestBias(combo, requestIntent);
            var score = baseScore + deterministicJitter - 2 + requestBias;

            var dominantStyle = styles
                .GroupBy(x => x)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "casual";

            var styleLabel = dominantStyle.Replace('_', ' ');
            var explanation = "The outfit balances color, silhouette, and formality across top, bottom, and shoes.";

            var places = new List<string>();
            if (!string.IsNullOrWhiteSpace(combo.Shirt.Occasion)) places.Add(combo.Shirt.Occasion!);
            if (!string.IsNullOrWhiteSpace(combo.Pants.Occasion)) places.Add(combo.Pants.Occasion!);
            if (!string.IsNullOrWhiteSpace(combo.Shoes.Occasion)) places.Add(combo.Shoes.Occasion!);
            var recommendedPlaces = places.Count > 0
                ? string.Join(", ", places.Distinct(StringComparer.OrdinalIgnoreCase))
                : "daily, coffee, city walk";

            return new ScoredCombination
            {
                ShirtId = combo.Shirt.GarmentId,
                PantsId = combo.Pants.GarmentId,
                ShoesId = combo.Shoes.GarmentId,
                Score = Math.Clamp(score, 0, 100),
                Rank = rank,
                StyleLabel = styleLabel,
                Explanation = explanation,
                RecommendedPlaces = recommendedPlaces
            };
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static int ComputeRequestBias(Combination combo, RequestIntent requestIntent)
        {
            if (requestIntent.Tokens.Count == 0)
                return 0;

            var tags = BuildComboTags(combo);
            if (tags.Count == 0)
                return -12;

            var matched = requestIntent.Tokens.Count(tags.Contains);
            var ratio = matched / (double)requestIntent.Tokens.Count;
            var weighted = (int)Math.Round((ratio * 38d) - 10d);

            if (requestIntent.WantsBlackDominant)
            {
                var blackPieces = CountBlackPieces(combo);
                weighted += blackPieces switch
                {
                    3 => 32,
                    2 => 8,
                    1 => -16,
                    _ => -30
                };
            }

            if (requestIntent.WantsSporty)
            {
                var sportyBrands = CountSportBrandPieces(combo);
                var luxuryBrands = CountLuxuryBrandPieces(combo);
                if (sportyBrands > 0)
                    weighted += 10 + (sportyBrands * 5);

                if (luxuryBrands > 0 && sportyBrands == 0)
                    weighted -= 18 + (luxuryBrands * 5);
                else if (luxuryBrands > 0)
                    weighted -= luxuryBrands * 6;
            }

            return Math.Clamp(weighted, -45, 58);
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static int CountBlackPieces(Combination combo)
        {
            var count = 0;
            if (IsBlackGarment(combo.Shirt)) count++;
            if (IsBlackGarment(combo.Pants)) count++;
            if (IsBlackGarment(combo.Shoes)) count++;
            return count;
        }

        // הסבר: פונקציית בדיקה. מחזירה תשובה לוגית/ניסיון פריסה כדי להחליט על המשך הזרימה.
        private static bool IsBlackGarment(Garment garment) =>
            IsBlackColor(garment.Color) || IsBlackColor(garment.ColorSecondary);

        // הסבר: פונקציית בדיקה. מחזירה תשובה לוגית/ניסיון פריסה כדי להחליט על המשך הזרימה.
        private static bool IsBlackColor(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var normalized = value.Trim().ToLowerInvariant();
            if (BlackColorKeywords.Contains(normalized))
                return true;

            return SplitToTokens(normalized).Any(BlackColorKeywords.Contains);
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static int CountSportBrandPieces(Combination combo)
        {
            var count = 0;
            if (IsSportBrand(combo.Shirt.Brand)) count++;
            if (IsSportBrand(combo.Pants.Brand)) count++;
            if (IsSportBrand(combo.Shoes.Brand)) count++;
            return count;
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static int CountLuxuryBrandPieces(Combination combo)
        {
            var count = 0;
            if (IsLuxuryBrand(combo.Shirt.Brand)) count++;
            if (IsLuxuryBrand(combo.Pants.Brand)) count++;
            if (IsLuxuryBrand(combo.Shoes.Brand)) count++;
            return count;
        }

        // הסבר: פונקציית בדיקה. מחזירה תשובה לוגית/ניסיון פריסה כדי להחליט על המשך הזרימה.
        private static bool IsSportBrand(string? brand) =>
            IsBrandMatch(brand, SportBrandKeywords);

        // הסבר: פונקציית בדיקה. מחזירה תשובה לוגית/ניסיון פריסה כדי להחליט על המשך הזרימה.
        private static bool IsLuxuryBrand(string? brand) =>
            IsBrandMatch(brand, LuxuryBrandKeywords);

        // הסבר: פונקציית בדיקה. מחזירה תשובה לוגית/ניסיון פריסה כדי להחליט על המשך הזרימה.
        private static bool IsBrandMatch(string? brand, HashSet<string> keywords)
        {
            if (string.IsNullOrWhiteSpace(brand))
                return false;

            var normalized = brand.Trim().ToLowerInvariant();
            if (keywords.Contains(normalized))
                return true;

            foreach (var keyword in keywords)
            {
                if (keyword.Contains(' '))
                {
                    if (normalized.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return SplitToTokens(normalized).Any(token => keywords.Contains(token));
        }

        // הסבר: פונקציית בנייה. מרכיבה אובייקט/בקשה/פלט מתוך נתוני קלט לפני השלב הבא בזרימה.
        private static HashSet<string> BuildComboTags(Combination combo)
        {
            var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            AddTags(tags, NormalizeTypeName(combo.Shirt.Type));
            AddTags(tags, NormalizeTypeName(combo.Pants.Type));
            AddTags(tags, NormalizeTypeName(combo.Shoes.Type));

            AddTags(tags, combo.Shirt.Color, combo.Shirt.ColorSecondary, combo.Shirt.Pattern, combo.Shirt.StyleCategory, combo.Shirt.Season, combo.Shirt.Occasion, combo.Shirt.Fit, combo.Shirt.Material, combo.Shirt.Brand);
            AddTags(tags, combo.Pants.Color, combo.Pants.ColorSecondary, combo.Pants.Pattern, combo.Pants.StyleCategory, combo.Pants.Season, combo.Pants.Occasion, combo.Pants.Fit, combo.Pants.Material, combo.Pants.Brand);
            AddTags(tags, combo.Shoes.Color, combo.Shoes.ColorSecondary, combo.Shoes.Pattern, combo.Shoes.StyleCategory, combo.Shoes.Season, combo.Shoes.Occasion, combo.Shoes.Fit, combo.Shoes.Material, combo.Shoes.Brand);

            foreach (var tag in ParseStyleTags(combo.Shirt.StyleTags, combo.Shirt.FeatureJson))
                AddTags(tags, tag);
            foreach (var tag in ParseStyleTags(combo.Pants.StyleTags, combo.Pants.FeatureJson))
                AddTags(tags, tag);
            foreach (var tag in ParseStyleTags(combo.Shoes.StyleTags, combo.Shoes.FeatureJson))
                AddTags(tags, tag);

            return tags;
        }

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        private static void AddTags(HashSet<string> set, params string?[] values)
        {
            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                var normalized = value.Trim().ToLowerInvariant();
                AddExpandedToken(set, normalized);

                foreach (var token in SplitToTokens(normalized))
                    AddExpandedToken(set, token);
            }
        }

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        private static void AddExpandedToken(HashSet<string> set, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return;

            var normalized = token.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalized))
                return;

            set.Add(normalized);

            if (CanonicalTagByAlias.TryGetValue(normalized, out var canonical))
                set.Add(canonical);
        }

        // הסבר: פונקציית פירסור. ממירה טקסט/JSON/פרמטרים למבנה נתונים שנוח לעבוד איתו.
        private static RequestIntent ParseRequestIntent(string? userRequest)
        {
            if (string.IsNullOrWhiteSpace(userRequest))
                return RequestIntent.Empty;

            var normalizedRequest = userRequest.Trim().ToLowerInvariant();
            var stopWords = new HashSet<string>(new[]
            {
                "a", "an", "the", "for", "to", "of", "and", "or", "with", "without", "in", "on", "at", "my", "me",
                "i", "want", "need", "look", "outfit", "please", "make", "give", "best", "recommendation", "recommendations"
            }, StringComparer.OrdinalIgnoreCase);

            var tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var token in SplitToTokens(userRequest))
            {
                if (token.Length < 2 || stopWords.Contains(token))
                    continue;

                AddExpandedToken(tokens, token);
            }

            var wantsBlackDominant =
                tokens.Contains("black") ||
                normalizedRequest.Contains("all black", StringComparison.OrdinalIgnoreCase) ||
                normalizedRequest.Contains("black outfit", StringComparison.OrdinalIgnoreCase);

            var wantsSporty =
                tokens.Contains("sporty") ||
                tokens.Contains("sport") ||
                tokens.Contains("athletic") ||
                tokens.Contains("gym") ||
                tokens.Contains("workout") ||
                tokens.Contains("running");

            return tokens.Count == 0
                ? RequestIntent.Empty
                : new RequestIntent(tokens, wantsBlackDominant, wantsSporty);
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static IEnumerable<string> SplitToTokens(string value)
        {
            var text = value.ToLowerInvariant();
            var buffer = new List<char>(text.Length);
            foreach (var ch in text)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    buffer.Add(ch);
                    continue;
                }

                buffer.Add(' ');
            }

            return new string(buffer.ToArray())
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static List<ScoredCombination> EnsureDescendingDistinctScores(List<ScoredCombination> sortedByScoreDesc)
        {
            if (sortedByScoreDesc.Count <= 1)
                return sortedByScoreDesc;

            var adjusted = new List<ScoredCombination>(sortedByScoreDesc.Count);
            var previousScore = 101;

            foreach (var item in sortedByScoreDesc)
            {
                var nextScore = Math.Clamp(item.Score, 0, 100);
                if (nextScore >= previousScore)
                    nextScore = Math.Max(0, previousScore - 1);

                previousScore = nextScore;
                adjusted.Add(item with { Score = nextScore });
            }

            return adjusted;
        }

        // הסבר: פונקציית בנייה. מרכיבה אובייקט/בקשה/פלט מתוך נתוני קלט לפני השלב הבא בזרימה.
        private static Dictionary<string, int> BuildTypeCounts(IEnumerable<Garment> garments)
        {
            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["shirt"] = 0,
                ["pants"] = 0,
                ["shoes"] = 0
            };

            foreach (var garment in garments)
            {
                var normalized = NormalizeTypeName(garment.Type);
                if (string.IsNullOrEmpty(normalized))
                    continue;

                if (!counts.ContainsKey(normalized))
                    counts[normalized] = 0;

                counts[normalized]++;
            }

            return counts;
        }

        // הסבר: פונקציית נרמול. מנקה ומאחד פורמט נתונים כדי למנוע חוסר עקביות בהמשך הזרימה.
        private static string NormalizeTypeName(string? type)
        {
            var value = (type ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Contains("shirt") || value.Contains("tee")) return "shirt";
            if (value.Contains("pant") || value.Contains("trouser") || value.Contains("jean")) return "pants";
            if (value.Contains("shoe") || value.Contains("sneaker") || value.Contains("boot")) return "shoes";
            return string.Empty;
        }

        // הסבר: פונקציית בדיקה. מחזירה תשובה לוגית/ניסיון פריסה כדי להחליט על המשך הזרימה.
        private static bool IsType(Garment g, string type) =>
            string.Equals(NormalizeTypeName(g.Type), type, StringComparison.OrdinalIgnoreCase);

        // הסבר: פונקציית בנייה. מרכיבה אובייקט/בקשה/פלט מתוך נתוני קלט לפני השלב הבא בזרימה.
        private static Dictionary<string, Garment> BuildGarmentLookup(IEnumerable<Garment> garments)
        {
            var lookup = new Dictionary<string, Garment>(StringComparer.OrdinalIgnoreCase);
            foreach (var garment in garments)
            {
                if (string.IsNullOrWhiteSpace(garment.GarmentId))
                    continue;

                lookup[garment.GarmentId] = garment;
            }

            return lookup;
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static List<Garment> MergeGarments(IEnumerable<Garment> cachedGarments, IEnumerable<Garment> authoritativeGarments)
        {
            var merged = BuildGarmentLookup(cachedGarments);
            foreach (var garment in authoritativeGarments)
            {
                if (string.IsNullOrWhiteSpace(garment.GarmentId))
                    continue;

                merged[garment.GarmentId] = garment;
            }

            return merged.Values.ToList();
        }
    }

    public record MatchingGateResult
    {
        public bool Allowed { get; init; }
        public int MinPerTypeRequired { get; init; }
        public Dictionary<string, int> Counts { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    }

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

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        public static MatchResult CreateBlocked(int min, Dictionary<string, int> counts) =>
            new() { Blocked = true, MinPerTypeRequired = min, Counts = counts };

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        public static MatchResult CreateNoMatches(string? message = null) =>
            new() { NoEligible = true, NoEligibleMessage = message };

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        public static MatchResult CreateError(string msg) => new() { ErrorMessage = msg };

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        public static MatchResult CreateSuccess(List<ScoredCombination> results) =>
            new() { Success = true, Results = results };
    }

    public record SaveOutfitResult
    {
        public bool Success { get; init; }
        public bool Duplicate { get; init; }
        public string? OutfitId { get; init; }
        public string Message { get; init; } = string.Empty;

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public static SaveOutfitResult SuccessResult(string outfitId) =>
            new() { Success = true, OutfitId = outfitId, Message = "Saved." };

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public static SaveOutfitResult DuplicateResult(string message) =>
            new() { Duplicate = true, Message = message };

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public static SaveOutfitResult Fail(string message) =>
            new() { Success = false, Duplicate = false, Message = message };
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

    internal class Combination
    {
        public Garment Shirt { get; init; } = default!;
        public Garment Pants { get; init; } = default!;
        public Garment Shoes { get; init; } = default!;
    }

    // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
    internal record RequestIntent(
        HashSet<string> Tokens,
        bool WantsBlackDominant,
        bool WantsSporty)
    {
        public static RequestIntent Empty { get; } = new(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            false,
            false);
    }

    // הסבר: פונקציית resolve. מחליטה מה הערך/המקור הנכון לשימוש לפי סדר עדיפויות ברור.
    internal record ResolvedRecommendation(ScoredCombination Recommendation, Combination Combo);

    internal record ScoringOutcome
    {
        public List<ScoredCombination> Results { get; init; } = new();
        public string? NoMatchMessage { get; init; }
        public string? ErrorMessage { get; init; }

        public static ScoringOutcome Empty { get; } = new();

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public static ScoringOutcome WithResults(List<ScoredCombination> results) =>
            new() { Results = results };

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public static ScoringOutcome WithNoMatch(string message) =>
            new() { NoMatchMessage = message };

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public static ScoringOutcome WithError(string message) =>
            new() { ErrorMessage = message };
    }
}
