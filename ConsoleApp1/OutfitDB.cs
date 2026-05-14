// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;

namespace DBL
{
    public class OutfitDB : BaseDB<Outfit>
    {
        private HashSet<string>? _outfitColumns;
        private HashSet<string>? _outfitGarmentColumns;
        private HashSet<string>? _garmentColumns;

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        protected override string GetPrimaryKeyName() => "outfit_id";
        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        protected override string GetTableName() => "eitan_project12.outfits";

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        protected override Task<Outfit> CreateModelAsync(object[] row)
        {
            // Canonical projection used by this class:
            // 0 outfit_id
            // 1 user_id
            // 2 shirt_garment_id
            // 3 pants_garment_id
            // 4 shoes_garment_id
            // 5 score
            // 6 rank
            // 7 style_label
            // 8 explanation
            // 9 recommended_places
            // 10 seed_type
            // 11 label_is_compatible
            // 12 label_source
            // 13 requested_garment_ids (or seed_garment_id fallback)
            // 14 created_at
            if (row.Length >= 15)
            {
                var requestedOrSeed = row[13]?.ToString();
                var firstRequested = FirstRequestedId(requestedOrSeed);
                return Task.FromResult(new Outfit
                {
                    OutfitId = row[0]?.ToString() ?? string.Empty,
                    UserId = row[1]?.ToString() ?? string.Empty,
                    ShirtGarmentId = row[2]?.ToString(),
                    PantsGarmentId = row[3]?.ToString(),
                    ShoesGarmentId = row[4]?.ToString(),
                    Score = ParseScore(row[5]),
                    Rank = int.TryParse(row[6]?.ToString(), out var rank) ? rank : 1,
                    StyleLabel = row[7]?.ToString(),
                    Explanation = row[8]?.ToString(),
                    RecommendedPlaces = row[9]?.ToString(),
                    SeedType = row[10]?.ToString(),
                    LabelIsCompatible = Convert.ToInt32(row[11] ?? 0) != 0,
                    LabelSource = row[12]?.ToString() ?? string.Empty,
                    RequestedGarmentIds = requestedOrSeed,
                    SeedGarmentId = firstRequested,
                    CreatedAt = DateTime.TryParse(row[14]?.ToString(), out var createdProjected) ? createdProjected : DateTime.UtcNow
                });
            }

            // Existing 2026-01 schema.
            if (row.Length >= 11)
            {
                var requested = row[9]?.ToString();
                return Task.FromResult(new Outfit
                {
                    OutfitId = row[0]?.ToString() ?? string.Empty,
                    UserId = row[1]?.ToString() ?? string.Empty,
                    ShirtGarmentId = row[2]?.ToString(),
                    PantsGarmentId = row[3]?.ToString(),
                    ShoesGarmentId = row[4]?.ToString(),
                    Score = ParseScore(row[5]),
                    Rank = int.TryParse(row[6]?.ToString(), out var rank) ? rank : 1,
                    LabelIsCompatible = Convert.ToInt32(row[7] ?? 0) != 0,
                    LabelSource = row[8]?.ToString() ?? string.Empty,
                    RequestedGarmentIds = requested,
                    SeedGarmentId = FirstRequestedId(requested),
                    CreatedAt = DateTime.TryParse(row[10]?.ToString(), out var createdLegacy) ? createdLegacy : DateTime.UtcNow
                });
            }

            // Legacy schema:
            // outfit_id, label_is_compatible, label_source, score, outfit_style, explanation,
            // recommended_where, seed_type, seed_garment_id, created_at
            if (row.Length >= 10)
            {
                var seedGarmentId = row[8]?.ToString();
                return Task.FromResult(new Outfit
                {
                    OutfitId = row[0]?.ToString() ?? string.Empty,
                    LabelIsCompatible = Convert.ToInt32(row[1] ?? 0) != 0,
                    LabelSource = row[2]?.ToString() ?? string.Empty,
                    Score = ParseScore(row[3]),
                    StyleLabel = row[4]?.ToString(),
                    Explanation = row[5]?.ToString(),
                    RecommendedPlaces = row[6]?.ToString(),
                    SeedType = row[7]?.ToString(),
                    SeedGarmentId = seedGarmentId,
                    RequestedGarmentIds = seedGarmentId,
                    CreatedAt = DateTime.TryParse(row[9]?.ToString(), out var createdLegacySchema) ? createdLegacySchema : DateTime.UtcNow
                });
            }

            return Task.FromResult(new Outfit
            {
                OutfitId = row.Length > 0 ? row[0]?.ToString() ?? string.Empty : string.Empty,
                CreatedAt = DateTime.UtcNow
            });
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<List<Outfit>> GetAllAsync()
        {
            return await SelectAllAsync($"SELECT * FROM {GetTableName()}");
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<List<Outfit>> GetByUserAsync(string userId, int take = 50)
        {
            return await GetFilteredByUserAsync(userId, new OutfitFilterRequest(), take);
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<Outfit?> GetByIdAsync(string outfitId)
        {
            var sql = $"SELECT * FROM {GetTableName()} WHERE outfit_id = @id LIMIT 1";
            var rows = await SelectAllAsync(sql, new Dictionary<string, object> { ["id"] = outfitId });
            return rows.FirstOrDefault();
        }

        // הסבר: פונקציית מחיקה. מסירה נתון קיים ומחזירה תוצאה כדי לאשר שהפעולה הושלמה.
        public async Task<int> DeleteOutfitAsync(string outfitId)
        {
            if (string.IsNullOrWhiteSpace(outfitId))
                return 0;

            return await DeleteAsync(new Dictionary<string, object>
            {
                ["outfit_id"] = outfitId.Trim()
            });
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public async Task<int> CountByUserRoleAsync(string role)
        {
            var normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();
            var outfitColumns = await GetOutfitColumnsAsync();

            if (outfitColumns.Contains("user_id"))
            {
                var sql = $@"SELECT COUNT(*)
                             FROM {GetTableName()} o
                             INNER JOIN eitan_project12.users u ON u.user_id = o.user_id
                             WHERE LOWER(TRIM(COALESCE(u.role,''))) = @role";
                var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>
                {
                    ["role"] = normalizedRole
                });
                return ParseCount(rows);
            }

            var garmentColumns = await GetGarmentColumnsAsync();
            var ownerColumn = garmentColumns.Contains("owner_user_id") ? "owner_user_id" : "user_id";

            var legacySql = $@"SELECT COUNT(DISTINCT o.outfit_id)
                               FROM {GetTableName()} o
                               INNER JOIN eitan_project12.outfit_garments og ON og.outfit_id = o.outfit_id
                               INNER JOIN eitan_project12.garments g ON g.garment_id = og.garment_id
                               INNER JOIN eitan_project12.users u ON u.user_id = g.{ownerColumn}
                               WHERE LOWER(TRIM(COALESCE(u.role,''))) = @role";
            var legacyRows = await StingListSelectAllAsync(legacySql, new Dictionary<string, object>
            {
                ["role"] = normalizedRole
            });
            return ParseCount(legacyRows);
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<Dictionary<DateTime, int>> GetDailyCreatedCountsByUserRoleAsync(int days, string role)
        {
            var normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();
            var safeDays = Math.Clamp(days, 1, 365);
            var startUtc = DateTime.UtcNow.Date.AddDays(-(safeDays - 1));
            var endUtcExclusive = DateTime.UtcNow.Date.AddDays(1);
            var outfitColumns = await GetOutfitColumnsAsync();
            var createdExpr = outfitColumns.Contains("created_at") ? "o.created_at" : "CURRENT_TIMESTAMP";

            if (outfitColumns.Contains("user_id"))
            {
                var sql = $@"SELECT DATE({createdExpr}) AS day_key, COUNT(*) AS cnt
                             FROM {GetTableName()} o
                             INNER JOIN eitan_project12.users u ON u.user_id = o.user_id
                             WHERE {createdExpr} >= @startUtc
                               AND {createdExpr} < @endUtcExclusive
                               AND LOWER(TRIM(COALESCE(u.role,''))) = @role
                             GROUP BY DATE({createdExpr})
                             ORDER BY day_key";
                var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>
                {
                    ["startUtc"] = startUtc,
                    ["endUtcExclusive"] = endUtcExclusive,
                    ["role"] = normalizedRole
                });
                return ParseDailyCounts(rows);
            }

            var garmentColumns = await GetGarmentColumnsAsync();
            var ownerColumn = garmentColumns.Contains("owner_user_id") ? "owner_user_id" : "user_id";

            var legacySql = $@"SELECT DATE({createdExpr}) AS day_key, COUNT(DISTINCT o.outfit_id) AS cnt
                               FROM {GetTableName()} o
                               INNER JOIN eitan_project12.outfit_garments og ON og.outfit_id = o.outfit_id
                               INNER JOIN eitan_project12.garments g ON g.garment_id = og.garment_id
                               INNER JOIN eitan_project12.users u ON u.user_id = g.{ownerColumn}
                               WHERE {createdExpr} >= @startUtc
                                 AND {createdExpr} < @endUtcExclusive
                                 AND LOWER(TRIM(COALESCE(u.role,''))) = @role
                               GROUP BY DATE({createdExpr})
                               ORDER BY day_key";
            var legacyRows = await StingListSelectAllAsync(legacySql, new Dictionary<string, object>
            {
                ["startUtc"] = startUtc,
                ["endUtcExclusive"] = endUtcExclusive,
                ["role"] = normalizedRole
            });
            return ParseDailyCounts(legacyRows);
        }

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        public async Task<int> CreateAsync(Outfit o)
        {
            var outfitColumns = await GetOutfitColumnsAsync();
            if (outfitColumns.Count == 0)
                return 0;

            var seedGarmentId = FirstNonEmpty(o.SeedGarmentId, FirstRequestedId(o.RequestedGarmentIds));
            var requestedGarmentIds = !string.IsNullOrWhiteSpace(o.RequestedGarmentIds)
                ? o.RequestedGarmentIds
                : seedGarmentId;

            var fields = new Dictionary<string, object>
            {
                ["outfit_id"] = string.IsNullOrWhiteSpace(o.OutfitId) ? Guid.NewGuid().ToString() : o.OutfitId
            };

            if (outfitColumns.Contains("user_id"))
                fields["user_id"] = o.UserId;

            if (outfitColumns.Contains("shirt_garment_id"))
                fields["shirt_garment_id"] = (object?)o.ShirtGarmentId ?? DBNull.Value;
            if (outfitColumns.Contains("pants_garment_id"))
                fields["pants_garment_id"] = (object?)o.PantsGarmentId ?? DBNull.Value;
            if (outfitColumns.Contains("shoes_garment_id"))
                fields["shoes_garment_id"] = (object?)o.ShoesGarmentId ?? DBNull.Value;

            if (outfitColumns.Contains("score"))
                fields["score"] = o.Score;
            if (outfitColumns.Contains("rank"))
                fields["rank"] = o.Rank;

            if (outfitColumns.Contains("style_label"))
                fields["style_label"] = (object?)o.StyleLabel ?? DBNull.Value;
            if (outfitColumns.Contains("outfit_style"))
                fields["outfit_style"] = (object?)o.StyleLabel ?? DBNull.Value;

            if (outfitColumns.Contains("explanation"))
                fields["explanation"] = (object?)o.Explanation ?? DBNull.Value;

            if (outfitColumns.Contains("recommended_places"))
                fields["recommended_places"] = (object?)o.RecommendedPlaces ?? DBNull.Value;
            if (outfitColumns.Contains("recommended_where"))
                fields["recommended_where"] = (object?)o.RecommendedPlaces ?? DBNull.Value;
            if (outfitColumns.Contains("where_to_wear"))
                fields["where_to_wear"] = (object?)o.RecommendedPlaces ?? DBNull.Value;

            if (outfitColumns.Contains("seed_type"))
                fields["seed_type"] = (object?)o.SeedType ?? DBNull.Value;

            if (outfitColumns.Contains("seed_garment_id"))
                fields["seed_garment_id"] = (object?)seedGarmentId ?? DBNull.Value;

            if (outfitColumns.Contains("label_is_compatible"))
                fields["label_is_compatible"] = o.LabelIsCompatible ? 1 : 0;
            if (outfitColumns.Contains("label_source"))
                fields["label_source"] = string.IsNullOrWhiteSpace(o.LabelSource) ? "auto" : o.LabelSource;
            if (outfitColumns.Contains("requested_garment_ids"))
                fields["requested_garment_ids"] = (object?)requestedGarmentIds ?? DBNull.Value;
            if (outfitColumns.Contains("created_at"))
                fields["created_at"] = o.CreatedAt == default ? DateTime.UtcNow : o.CreatedAt;

            return await InsertAsync(fields);
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public async Task<bool> ExistsDuplicateAsync(
            string userId,
            string shirtGarmentId,
            string pantsGarmentId,
            string shoesGarmentId,
            string? seedType,
            string? seedGarmentId)
        {
            var outfitColumns = await GetOutfitColumnsAsync();
            var normalizedSeedType = (seedType ?? string.Empty).Trim().ToLowerInvariant();
            var normalizedSeedGarmentId = (seedGarmentId ?? string.Empty).Trim();

            if (outfitColumns.Contains("user_id") &&
                outfitColumns.Contains("shirt_garment_id") &&
                outfitColumns.Contains("pants_garment_id") &&
                outfitColumns.Contains("shoes_garment_id"))
            {
                var seedColumn = outfitColumns.Contains("seed_garment_id")
                    ? "seed_garment_id"
                    : outfitColumns.Contains("requested_garment_ids")
                        ? "requested_garment_ids"
                        : string.Empty;

                var sql = $@"SELECT outfit_id
                             FROM {GetTableName()}
                             WHERE user_id = @uid
                               AND shirt_garment_id = @shirt
                               AND pants_garment_id = @pants
                               AND shoes_garment_id = @shoes";

                var parameters = new Dictionary<string, object>
                {
                    ["uid"] = userId,
                    ["shirt"] = shirtGarmentId,
                    ["pants"] = pantsGarmentId,
                    ["shoes"] = shoesGarmentId
                };

                if (!string.IsNullOrWhiteSpace(normalizedSeedType) && outfitColumns.Contains("seed_type"))
                {
                    sql += " AND LOWER(TRIM(COALESCE(seed_type,''))) = @seedType";
                    parameters["seedType"] = normalizedSeedType;
                }

                if (!string.IsNullOrWhiteSpace(normalizedSeedGarmentId) && !string.IsNullOrWhiteSpace(seedColumn))
                {
                    sql += $" AND {seedColumn} = @seedGarmentId";
                    parameters["seedGarmentId"] = normalizedSeedGarmentId;
                }

                sql += " LIMIT 1";
                var rows = await StingListSelectAllAsync(sql, parameters);
                return rows.Count > 0;
            }

            var linkColumns = await GetOutfitGarmentColumnsAsync();
            var garmentColumns = await GetGarmentColumnsAsync();
            var ownerColumn = garmentColumns.Contains("owner_user_id") ? "owner_user_id" : "user_id";
            var roleExpr = BuildRoleNormalizationExpression(linkColumns, "og");
            var seedExpr = ResolveOutfitExpression(outfitColumns, "o.seed_type", "NULL");
            var seedGarmentExpr = ResolveOutfitExpression(outfitColumns, "o.seed_garment_id", "o.requested_garment_ids");

            var legacySql = $@"SELECT o.outfit_id
                               FROM {GetTableName()} o
                               WHERE EXISTS (
                                   SELECT 1
                                   FROM eitan_project12.outfit_garments ogu
                                   INNER JOIN eitan_project12.garments gu ON gu.garment_id = ogu.garment_id
                                   WHERE ogu.outfit_id = o.outfit_id
                                     AND gu.{ownerColumn} = @uid
                               )
                                 AND EXISTS (
                                   SELECT 1
                                   FROM eitan_project12.outfit_garments og
                                   WHERE og.outfit_id = o.outfit_id
                                     AND {roleExpr} = 'shirt'
                                     AND og.garment_id = @shirt
                               )
                                 AND EXISTS (
                                   SELECT 1
                                   FROM eitan_project12.outfit_garments og
                                   WHERE og.outfit_id = o.outfit_id
                                     AND {roleExpr} = 'pants'
                                     AND og.garment_id = @pants
                               )
                                 AND EXISTS (
                                   SELECT 1
                                   FROM eitan_project12.outfit_garments og
                                   WHERE og.outfit_id = o.outfit_id
                                     AND {roleExpr} = 'shoes'
                                     AND og.garment_id = @shoes
                               )";

            var legacyParams = new Dictionary<string, object>
            {
                ["uid"] = userId,
                ["shirt"] = shirtGarmentId,
                ["pants"] = pantsGarmentId,
                ["shoes"] = shoesGarmentId
            };

            if (!string.IsNullOrWhiteSpace(normalizedSeedType) && !string.Equals(seedExpr, "NULL", StringComparison.Ordinal))
            {
                legacySql += $" AND LOWER(TRIM(COALESCE({seedExpr},''))) = @seedType";
                legacyParams["seedType"] = normalizedSeedType;
            }

            if (!string.IsNullOrWhiteSpace(normalizedSeedGarmentId) && !string.Equals(seedGarmentExpr, "NULL", StringComparison.Ordinal))
            {
                legacySql += $" AND {seedGarmentExpr} = @seedGarmentId";
                legacyParams["seedGarmentId"] = normalizedSeedGarmentId;
            }

            legacySql += " LIMIT 1";
            var legacyRows = await StingListSelectAllAsync(legacySql, legacyParams);
            return legacyRows.Count > 0;
        }


        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<List<Outfit>> GetFilteredByUserAsync(string userId, OutfitFilterRequest? filter, int take = 200)
        {
            filter ??= new OutfitFilterRequest();
            filter.Normalize();

            var outfitColumns = await GetOutfitColumnsAsync();
            var rawOutfits = outfitColumns.Contains("user_id")
                ? await QueryModernBaseAsync(userId, filter, Math.Max(400, take * 4))
                : await QueryLegacyBaseAsync(userId, filter, Math.Max(400, take * 4));

            Dictionary<string, OutfitFacts>? factsByOutfit = null;
            var needsFacts =
                filter.GarmentTypes.Count > 0 ||
                filter.Occasions.Count > 0 ||
                filter.Seasons.Count > 0;

            if (needsFacts)
            {
                factsByOutfit = await QueryOutfitFactsByOutfitAsync(userId, rawOutfits.Select(x => x.OutfitId));
            }

            var filtered = rawOutfits
                .Where(o => MatchesOutfitFilter(o, filter, factsByOutfit))
                .OrderByDescending(o => o.CreatedAt)
                .Take(Math.Max(1, take))
                .ToList();

            return filtered;
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<OutfitFilterOptions> GetFilterOptionsAsync(string userId, OutfitFilterRequest? filter)
        {
            filter ??= new OutfitFilterRequest();
            filter.Normalize();

            var outfits = await GetFilteredByUserAsync(userId, filter, 1200);
            var options = new OutfitFilterOptions
            {
                SeedTypes = outfits
                    .Select(o => (o.SeedType ?? string.Empty).Trim().ToLowerInvariant())
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                StyleLabels = outfits
                    .Select(o => (o.StyleLabel ?? string.Empty).Trim().ToLowerInvariant())
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                RecommendedPlaces = outfits
                    .SelectMany(o => SplitRecommendationTokens(o.RecommendedPlaces))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                    .ToList()
            };

            var facts = await QueryOutfitFactsByOutfitAsync(userId, outfits.Select(o => o.OutfitId));
            options.GarmentTypes = facts.Values
                .SelectMany(v => v.GarmentTypes)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
            options.Occasions = facts.Values
                .SelectMany(v => v.Occasions)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
            options.Seasons = facts.Values
                .SelectMany(v => v.Seasons)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return options;
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private async Task<List<Outfit>> QueryModernBaseAsync(string userId, OutfitFilterRequest filter, int take)
        {
            var columns = await GetOutfitColumnsAsync();
            if (!columns.Contains("user_id"))
                return new List<Outfit>();
            var garmentColumns = await GetGarmentColumnsAsync();
            var ownerColumn = garmentColumns.Contains("owner_user_id") ? "owner_user_id" : "user_id";

            var scoreExpr = columns.Contains("score") ? "o.score" : "0";
            var rankExpr = columns.Contains("rank") ? "o.`rank`" : "1";
            var createdExpr = columns.Contains("created_at") ? "o.created_at" : "CURRENT_TIMESTAMP";
            var styleExpr = ResolveOutfitExpression(columns, "o.style_label", "o.outfit_style");
            var placesExpr = ResolveOutfitExpression(
                columns,
                "o.recommended_places",
                ResolveOutfitExpression(columns, "o.recommended_where", "o.where_to_wear"));
            var seedExpr = columns.Contains("seed_type") ? "o.seed_type" : "NULL";
            var explanationExpr = columns.Contains("explanation") ? "o.explanation" : "NULL";
            var labelCompatibleExpr = columns.Contains("label_is_compatible") ? "o.label_is_compatible" : "0";
            var labelSourceExpr = columns.Contains("label_source") ? "o.label_source" : "'auto'";
            var requestedExpr = ResolveOutfitExpression(columns, "o.requested_garment_ids", "o.seed_garment_id");
            var shirtExpr = columns.Contains("shirt_garment_id") ? "o.shirt_garment_id" : "NULL";
            var pantsExpr = columns.Contains("pants_garment_id") ? "o.pants_garment_id" : "NULL";
            var shoesExpr = columns.Contains("shoes_garment_id") ? "o.shoes_garment_id" : "NULL";

            var sql = $@"SELECT o.outfit_id,
                                o.user_id AS user_id,
                                {shirtExpr} AS shirt_garment_id,
                                {pantsExpr} AS pants_garment_id,
                                {shoesExpr} AS shoes_garment_id,
                                CAST(ROUND(COALESCE({scoreExpr}, 0)) AS SIGNED) AS score,
                                {rankExpr} AS rank_value,
                                {styleExpr} AS style_label,
                                {explanationExpr} AS explanation,
                                {placesExpr} AS recommended_places,
                                {seedExpr} AS seed_type,
                                {labelCompatibleExpr} AS label_is_compatible,
                                {labelSourceExpr} AS label_source,
                                {requestedExpr} AS requested_garment_ids,
                                {createdExpr} AS created_at
                         FROM {GetTableName()} o
                         WHERE (
                               o.user_id = @uid
                               OR EXISTS (
                                   SELECT 1
                                   FROM eitan_project12.outfit_garments ogu
                                   INNER JOIN eitan_project12.garments gu ON gu.garment_id = ogu.garment_id
                                   WHERE ogu.outfit_id = o.outfit_id
                                     AND gu.{ownerColumn} = @uid
                               )
                         )
                           AND COALESCE({scoreExpr}, 0) BETWEEN @minScore AND @maxScore
                         ORDER BY {createdExpr} DESC
                         LIMIT @take";

            return await SelectAllAsync(sql, new Dictionary<string, object>
            {
                ["uid"] = userId,
                ["minScore"] = filter.MinScore,
                ["maxScore"] = filter.MaxScore,
                ["take"] = Math.Max(1, take)
            });
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private async Task<List<Outfit>> QueryLegacyBaseAsync(string userId, OutfitFilterRequest filter, int take)
        {
            var outfitColumns = await GetOutfitColumnsAsync();
            var linkColumns = await GetOutfitGarmentColumnsAsync();
            var garmentColumns = await GetGarmentColumnsAsync();
            var ownerColumn = garmentColumns.Contains("owner_user_id") ? "owner_user_id" : "user_id";
            var roleExpr = BuildRoleNormalizationExpression(linkColumns, "og");

            var scoreExpr = outfitColumns.Contains("score") ? "o.score" : "0";
            var createdExpr = outfitColumns.Contains("created_at") ? "o.created_at" : "CURRENT_TIMESTAMP";
            var styleExpr = ResolveOutfitExpression(outfitColumns, "o.style_label", "o.outfit_style");
            var placesExpr = ResolveOutfitExpression(
                outfitColumns,
                "o.recommended_places",
                ResolveOutfitExpression(outfitColumns, "o.recommended_where", "o.where_to_wear"));
            var seedExpr = outfitColumns.Contains("seed_type") ? "o.seed_type" : "NULL";
            var explanationExpr = outfitColumns.Contains("explanation") ? "o.explanation" : "NULL";
            var labelCompatibleExpr = outfitColumns.Contains("label_is_compatible") ? "o.label_is_compatible" : "0";
            var labelSourceExpr = outfitColumns.Contains("label_source") ? "o.label_source" : "'auto'";
            var requestedExpr = ResolveOutfitExpression(outfitColumns, "o.requested_garment_ids", "o.seed_garment_id");

            var sql = $@"SELECT o.outfit_id,
                                @uid AS user_id,
                                MAX(CASE WHEN {roleExpr} = 'shirt' THEN og.garment_id END) AS shirt_garment_id,
                                MAX(CASE WHEN {roleExpr} = 'pants' THEN og.garment_id END) AS pants_garment_id,
                                MAX(CASE WHEN {roleExpr} = 'shoes' THEN og.garment_id END) AS shoes_garment_id,
                                MAX(CAST(ROUND(COALESCE({scoreExpr}, 0)) AS SIGNED)) AS score,
                                1 AS rank_value,
                                MAX({styleExpr}) AS style_label,
                                MAX({explanationExpr}) AS explanation,
                                MAX({placesExpr}) AS recommended_places,
                                MAX({seedExpr}) AS seed_type,
                                MAX({labelCompatibleExpr}) AS label_is_compatible,
                                MAX({labelSourceExpr}) AS label_source,
                                MAX({requestedExpr}) AS requested_garment_ids,
                                MAX({createdExpr}) AS created_at
                         FROM {GetTableName()} o
                         LEFT JOIN eitan_project12.outfit_garments og ON og.outfit_id = o.outfit_id
                         WHERE EXISTS (
                             SELECT 1
                             FROM eitan_project12.outfit_garments ogu
                             INNER JOIN eitan_project12.garments gu ON gu.garment_id = ogu.garment_id
                             WHERE ogu.outfit_id = o.outfit_id
                               AND gu.{ownerColumn} = @uid
                         )
                           AND COALESCE({scoreExpr}, 0) BETWEEN @minScore AND @maxScore
                         GROUP BY o.outfit_id
                         ORDER BY MAX({createdExpr}) DESC
                         LIMIT @take";

            return await SelectAllAsync(sql, new Dictionary<string, object>
            {
                ["uid"] = userId,
                ["minScore"] = filter.MinScore,
                ["maxScore"] = filter.MaxScore,
                ["take"] = Math.Max(1, take)
            });
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private async Task<Dictionary<string, OutfitFacts>> QueryOutfitFactsByOutfitAsync(string userId, IEnumerable<string> outfitIds)
        {
            var ids = outfitIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (ids.Count == 0)
                return new Dictionary<string, OutfitFacts>(StringComparer.OrdinalIgnoreCase);

            var linkColumns = await GetOutfitGarmentColumnsAsync();
            var garmentColumns = await GetGarmentColumnsAsync();
            if (linkColumns.Count == 0)
                return new Dictionary<string, OutfitFacts>(StringComparer.OrdinalIgnoreCase);

            var roleExpr = BuildRoleNormalizationExpression(linkColumns, "og");
            var ownerColumn = garmentColumns.Contains("owner_user_id") ? "owner_user_id" : "user_id";
            var hasOccasion = garmentColumns.Contains("occasion");
            var hasSeason = garmentColumns.Contains("season");

            var placeholders = string.Join(",", ids.Select((_, i) => $"@id{i}"));
            var parameters = new Dictionary<string, object> { ["uid"] = userId };
            for (var i = 0; i < ids.Count; i++)
                parameters[$"id{i}"] = ids[i];

            var occasionExpr = hasOccasion ? "LOWER(TRIM(COALESCE(g.occasion,'')))" : "''";
            var seasonExpr = hasSeason ? "LOWER(TRIM(COALESCE(g.season,'')))" : "''";

            var sql = $@"SELECT og.outfit_id,
                                {roleExpr} AS garment_type,
                                {occasionExpr} AS occasion,
                                {seasonExpr} AS season
                         FROM eitan_project12.outfit_garments og
                         INNER JOIN eitan_project12.garments g ON g.garment_id = og.garment_id
                         WHERE og.outfit_id IN ({placeholders})
                           AND g.{ownerColumn} = @uid";

            var rows = await StingListSelectAllAsync(sql, parameters);
            var result = new Dictionary<string, OutfitFacts>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                if (row.Length < 4)
                    continue;

                var outfitId = row[0]?.ToString();
                if (string.IsNullOrWhiteSpace(outfitId))
                    continue;

                if (!result.TryGetValue(outfitId, out var facts))
                {
                    facts = new OutfitFacts();
                    result[outfitId] = facts;
                }

                var type = NormalizeTypeName(row[1]?.ToString());
                if (!string.IsNullOrWhiteSpace(type))
                    facts.GarmentTypes.Add(type);

                var occasion = (row[2]?.ToString() ?? string.Empty).Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(occasion))
                    facts.Occasions.Add(occasion);

                var season = (row[3]?.ToString() ?? string.Empty).Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(season))
                    facts.Seasons.Add(season);
            }

            return result;
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static bool MatchesOutfitFilter(
            Outfit outfit,
            OutfitFilterRequest filter,
            Dictionary<string, OutfitFacts>? factsByOutfit)
        {
            if (outfit.Score < filter.MinScore || outfit.Score > filter.MaxScore)
                return false;

            if (filter.SeedTypes.Count > 0)
            {
                var value = (outfit.SeedType ?? string.Empty).Trim().ToLowerInvariant();
                if (!filter.SeedTypes.Contains(value, StringComparer.OrdinalIgnoreCase))
                    return false;
            }

            if (filter.StyleLabels.Count > 0)
            {
                var value = (outfit.StyleLabel ?? string.Empty).Trim().ToLowerInvariant();
                if (!filter.StyleLabels.Contains(value, StringComparer.OrdinalIgnoreCase))
                    return false;
            }

            if (filter.RecommendedPlaces.Count > 0)
            {
                var tokens = SplitRecommendationTokens(outfit.RecommendedPlaces);
                var match = tokens.Any(token => filter.RecommendedPlaces.Any(sel => token.Contains(sel, StringComparison.OrdinalIgnoreCase)));
                if (!match)
                    return false;
            }

            if (factsByOutfit == null ||
                (!filter.GarmentTypes.Any() && !filter.Occasions.Any() && !filter.Seasons.Any()))
            {
                return true;
            }

            if (!factsByOutfit.TryGetValue(outfit.OutfitId, out var facts))
                return false;

            if (filter.GarmentTypes.Count > 0)
            {
                var selectedTypes = filter.GarmentTypes
                    .Select(NormalizeTypeName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (!facts.GarmentTypes.Overlaps(selectedTypes))
                    return false;
            }

            if (filter.Occasions.Count > 0 && !facts.Occasions.Overlaps(filter.Occasions))
                return false;

            if (filter.Seasons.Count > 0 && !facts.Seasons.Overlaps(filter.Seasons))
                return false;

            return true;
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        private async Task<HashSet<string>> GetOutfitColumnsAsync()
        {
            if (_outfitColumns != null)
                return _outfitColumns;

            _outfitColumns = await QueryColumnSetAsync("outfits");
            return _outfitColumns;
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        private async Task<HashSet<string>> GetOutfitGarmentColumnsAsync()
        {
            if (_outfitGarmentColumns != null)
                return _outfitGarmentColumns;

            _outfitGarmentColumns = await QueryColumnSetAsync("outfit_garments");
            return _outfitGarmentColumns;
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        private async Task<HashSet<string>> GetGarmentColumnsAsync()
        {
            if (_garmentColumns != null)
                return _garmentColumns;

            _garmentColumns = await QueryColumnSetAsync("garments");
            return _garmentColumns;
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private async Task<HashSet<string>> QueryColumnSetAsync(string tableName)
        {
            var sql = @"SELECT LOWER(column_name)
                        FROM information_schema.columns
                        WHERE table_schema = 'eitan_project12'
                          AND table_name = @tableName";

            var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>
            {
                ["tableName"] = tableName
            });

            return rows
                .Select(r => r.Length > 0 ? r[0]?.ToString() ?? string.Empty : string.Empty)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim().ToLowerInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        // הסבר: פונקציית resolve. מחליטה מה הערך/המקור הנכון לשימוש לפי סדר עדיפויות ברור.
        private static string ResolveOutfitExpression(HashSet<string> columns, string primaryExpression, string fallbackExpression)
        {
            var primaryColumn = ExtractColumnName(primaryExpression);
            if (columns.Contains(primaryColumn))
                return primaryExpression;

            var fallbackColumn = ExtractColumnName(fallbackExpression);
            if (columns.Contains(fallbackColumn))
                return fallbackExpression;

            return "NULL";
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static string ExtractColumnName(string expression)
        {
            var value = expression;
            var dot = value.LastIndexOf('.');
            if (dot >= 0)
                value = value[(dot + 1)..];
            var space = value.IndexOf(' ');
            if (space >= 0)
                value = value[..space];
            return value.Trim().ToLowerInvariant();
        }

        // הסבר: פונקציית בנייה. מרכיבה אובייקט/בקשה/פלט מתוך נתוני קלט לפני השלב הבא בזרימה.
        private static string BuildRoleNormalizationExpression(HashSet<string> linkColumns, string alias)
        {
            var roleSource = linkColumns.Contains("garment_type")
                ? $"LOWER(TRIM({alias}.garment_type))"
                : linkColumns.Contains("role")
                    ? $"LOWER(TRIM({alias}.role))"
                    : "''";

            return $@"(CASE
                        WHEN {roleSource} LIKE '%shirt%' OR {roleSource} LIKE '%tee%' OR {roleSource} LIKE '%top%' THEN 'shirt'
                        WHEN {roleSource} LIKE '%pant%' OR {roleSource} LIKE '%trouser%' OR {roleSource} LIKE '%jean%' OR {roleSource} LIKE '%bottom%' THEN 'pants'
                        WHEN {roleSource} LIKE '%shoe%' OR {roleSource} LIKE '%sneaker%' OR {roleSource} LIKE '%boot%' OR {roleSource} LIKE '%foot%' THEN 'shoes'
                        ELSE {roleSource}
                      END)";
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static IEnumerable<string> SplitRecommendationTokens(string? places)
        {
            if (string.IsNullOrWhiteSpace(places))
                return Enumerable.Empty<string>();

            return places
                .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => x.Trim().ToLowerInvariant())
                .Where(x => !string.IsNullOrWhiteSpace(x));
        }

        // הסבר: פונקציית פירסור. ממירה טקסט/JSON/פרמטרים למבנה נתונים שנוח לעבוד איתו.
        private static int ParseScore(object? value)
        {
            if (value == null)
                return 0;

            if (value is int i)
                return Math.Clamp(i, 0, 100);

            if (value is long l)
                return Math.Clamp((int)l, 0, 100);

            if (value is decimal d)
                return Math.Clamp((int)Math.Round(d), 0, 100);

            if (value is double db)
                return Math.Clamp((int)Math.Round(db), 0, 100);

            if (int.TryParse(value.ToString(), out var parsedInt))
                return Math.Clamp(parsedInt, 0, 100);

            if (decimal.TryParse(value.ToString(), out var parsedDecimal))
                return Math.Clamp((int)Math.Round(parsedDecimal), 0, 100);

            return 0;
        }

        // הסבר: פונקציית פירסור. ממירה טקסט/JSON/פרמטרים למבנה נתונים שנוח לעבוד איתו.
        private static int ParseCount(List<object[]> rows)
        {
            if (rows.Count == 0 || rows[0].Length == 0)
                return 0;

            var raw = rows[0][0]?.ToString();
            if (long.TryParse(raw, out var asLong))
                return (int)Math.Clamp(asLong, 0, int.MaxValue);

            if (int.TryParse(raw, out var asInt))
                return Math.Max(0, asInt);

            return 0;
        }

        // הסבר: פונקציית פירסור. ממירה טקסט/JSON/פרמטרים למבנה נתונים שנוח לעבוד איתו.
        private static Dictionary<DateTime, int> ParseDailyCounts(List<object[]> rows)
        {
            var result = new Dictionary<DateTime, int>();
            foreach (var row in rows)
            {
                if (row.Length < 2)
                    continue;

                if (!DateTime.TryParse(row[0]?.ToString(), out var day))
                    continue;

                var raw = row[1]?.ToString();
                var count = 0;
                if (long.TryParse(raw, out var asLong))
                    count = (int)Math.Clamp(asLong, 0, int.MaxValue);
                else if (int.TryParse(raw, out var asInt))
                    count = Math.Max(0, asInt);

                result[day.Date] = count;
            }

            return result;
        }

        // הסבר: פונקציית נרמול. מנקה ומאחד פורמט נתונים כדי למנוע חוסר עקביות בהמשך הזרימה.
        private static string NormalizeTypeName(string? type)
        {
            var value = (type ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            if (value.Contains("shirt") || value.Contains("tee") || value.Contains("top"))
                return "shirt";
            if (value.Contains("pant") || value.Contains("trouser") || value.Contains("jean") || value.Contains("bottom"))
                return "pants";
            if (value.Contains("shoe") || value.Contains("sneaker") || value.Contains("boot") || value.Contains("foot"))
                return "shoes";
            return value;
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static string? FirstRequestedId(string? requestedGarmentIds)
        {
            if (string.IsNullOrWhiteSpace(requestedGarmentIds))
                return null;

            return requestedGarmentIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private static string? FirstNonEmpty(params string?[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return null;
        }

        private sealed class OutfitFacts
        {
            public HashSet<string> GarmentTypes { get; } = new(StringComparer.OrdinalIgnoreCase);
            public HashSet<string> Occasions { get; } = new(StringComparer.OrdinalIgnoreCase);
            public HashSet<string> Seasons { get; } = new(StringComparer.OrdinalIgnoreCase);
        }
    }
}
