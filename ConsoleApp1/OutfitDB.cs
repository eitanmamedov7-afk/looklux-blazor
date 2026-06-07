
// SEARCH INDEX
// DATABASE, OUTFIT, RECOMMENDATION, MATCH, SAVE, FILTER, SEARCH, ADD, REMOVE, COUNT, DUPLICATE
//
// Topic: OUTFIT DATABASE
// Purpose: Stores saved recommendation rows and supports saved-outfit display, filters, duplicates, and counts.
// Search keywords: DATABASE OUTFIT RECOMMENDATION MATCH SAVE FILTER SEARCH ADD REMOVE COUNT DUPLICATE
// When to use it: Show this when explaining saved outfits, outfit filters, admin/dashboard counts, or duplicate prevention.
// Important notes: Outfit garment ids are linked through outfit_garments; this table stores outfit metadata.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;

namespace DBL
{
    // SECTION: OUTFIT DATABASE RECOMMENDATION
    // Topic: OutfitDB class
    // Purpose: Table-specific DB logic for eitan_project12.outfits.
    // Search keywords: DATABASE OUTFIT RECOMMENDATION FILTER COUNT DUPLICATE
    // When to use it: Use for saving/retrieving saved outfit recommendations and their filter options.
    // Important notes: Query methods often join outfit_garments to reconstruct shirt/pants/shoes ids.
    // Data-access class for the outfits table.
    // This class is used when outfit recommendations are saved, when the Outfits page loads saved outfits,
    // and when admin/dashboard pages count outfit activity.
    // The outfits table stores only outfit metadata; the actual shirt/pants/shoes garment ids live in outfit_garments.
    // Project process affected: recommendation saving, saved-outfit display, outfit filters, and admin statistics.
    public class OutfitDB : BaseDB<Outfit>
    {
        // Cached list of garment table columns. This affects all outfit reads that must find which user owns an outfit.
        // It lets this class prefer user_id while still reading older databases that use owner_user_id.
        private HashSet<string>? _garmentColumns;

        // BaseDB uses this primary key for generic load/delete/update helper methods.
        protected override string GetPrimaryKeyName() => "outfit_id";

        // Real database table that stores one saved outfit recommendation row.
        protected override string GetTableName() => "eitan_project12.outfits";

        // Converts database result rows into the Outfit model used by the saved outfits UI and API endpoints.
        // Some queries below project joined data into extra model fields that are not real columns in outfits.
        protected override Task<Outfit> CreateModelAsync(object[] row)
        {
            if (row.Length >= 15)
            {
                // This shape comes from QueryBaseAsync for the saved outfits/filter screen:
                // outfit metadata plus shirt/pants/shoes ids from outfit_garments.
                var requestedOrSeed = row[13]?.ToString();

                // The UI model still has RequestedGarmentIds, but the current table stores one seed_garment_id.
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

            if (row.Length >= 11)
            {
                // Older projected query shape kept for any older outfit display/admin code that still returns it.
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

            if (row.Length >= 10)
            {
                // Direct SELECT * from the current outfits table, used by simple load-by-id/all flows.
                // This row does not contain shirt/pants/shoes ids because those are in outfit_garments.
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

            // Minimal fallback for defensive parsing if a partial row is ever returned.
            return Task.FromResult(new Outfit
            {
                OutfitId = row.Length > 0 ? row[0]?.ToString() ?? string.Empty : string.Empty,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Returns raw outfit rows from the outfits table only.
        // This affects broad admin/debug listing flows, not the normal user outfit page.
        // Normal user outfit display should use GetFilteredByUserAsync so it also gets linked garment ids.
        // Use carefully: raw outfits do not include shirt/pants/shoes ids from outfit_garments.
        public async Task<List<Outfit>> GetAllAsync()
        {
            return await SelectAllAsync($"SELECT * FROM {GetTableName()}");
        }

        // Returns the user's outfits for the saved outfits page through the normal filter path,
        // because outfits has no user_id column.
        // Project process affected: web Outfits page, mobile saved outfits screen, and admin customer outfit view.
        // FLOW_OUTFIT_VIEW_WEB_03 / FLOW_OUTFIT_VIEW_MOBILE_05 / FLOW_DELETE_ACCOUNT_WEB_03 / FLOW_DELETE_ACCOUNT_MOBILE_05:
        // OutfitDB.GetByUserAsync loads saved outfits for one user.
        // This DB file is involved because saved recommendations live in outfits; next step is UI rendering or cascade delete.
        public async Task<List<Outfit>> GetByUserAsync(string userId, int take = 50)
        {
            return await GetFilteredByUserAsync(userId, new OutfitFilterRequest(), take);
        }

        // Loads one outfit by primary key for detail/admin flows.
        // This returns the metadata row only; callers that need garment links use OutfitGarmentDB.
        // This is not the normal saved-outfit list path.
        public async Task<Outfit?> GetByIdAsync(string outfitId)
        {
            var sql = $"SELECT * FROM {GetTableName()} WHERE outfit_id = @id LIMIT 1";
            var rows = await SelectAllAsync(sql, new Dictionary<string, object> { ["id"] = outfitId });
            return rows.FirstOrDefault();
        }

        // Deletes the outfit metadata row for delete-outfit flows.
        // Linked outfit_garments rows should be handled by DB constraints or caller flow.
        // FLOW_DELETE_OUTFIT_WEB_04 / FLOW_DELETE_OUTFIT_MOBILE_05 / FLOW_DELETE_ACCOUNT_WEB_04 / FLOW_DELETE_ACCOUNT_MOBILE_05:
        // OutfitDB.DeleteOutfitAsync removes the outfit row after outfit_garments links are deleted.
        // This DB file is involved because referential cleanup must delete child links first; next step is refresh/API response.
        public async Task<int> DeleteOutfitAsync(string outfitId)
        {
            // Empty ids are ignored so delete buttons cannot accidentally delete more than intended.
            if (string.IsNullOrWhiteSpace(outfitId))
                return 0;

            // BaseDB builds DELETE FROM outfits WHERE outfit_id = ...
            return await DeleteAsync(new Dictionary<string, object>
            {
                ["outfit_id"] = outfitId.Trim()
            });
        }

        // Counts outfits for dashboard/admin stats by user role.
        // It joins through outfit_garments and garments because outfits does not store user_id.
        public async Task<int> CountByUserRoleAsync(string role)
        {
            var normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();
            var garmentColumns = await GetGarmentColumnsAsync();

            // The merged garments table uses user_id; older databases used owner_user_id.
            var ownerColumn = garmentColumns.Contains("user_id") ? "user_id" : "owner_user_id";

            // Count distinct outfits because one outfit has multiple outfit_garments rows.
            var sql = $@"SELECT COUNT(DISTINCT o.outfit_id)
                         FROM {GetTableName()} o
                         INNER JOIN eitan_project12.outfit_garments og ON og.outfit_id = o.outfit_id
                         INNER JOIN eitan_project12.garments g ON g.garment_id = og.garment_id
                         INNER JOIN eitan_project12.users u ON u.user_id = g.{ownerColumn}
                         WHERE LOWER(TRIM(COALESCE(u.role,''))) = @role";
            var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>
            {
                ["role"] = normalizedRole
            });
            return ParseCount(rows);
        }

        // Builds daily outfit-created counts for dashboard/stat screens.
        // This affects charts that show how many outfit recommendations were created over time.
        public async Task<Dictionary<DateTime, int>> GetDailyCreatedCountsByUserRoleAsync(int days, string role)
        {
            var normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();

            // Limit the chart range to a safe size even if the UI sends a bad value.
            var safeDays = Math.Clamp(days, 1, 365);
            var startUtc = DateTime.UtcNow.Date.AddDays(-(safeDays - 1));
            var endUtcExclusive = DateTime.UtcNow.Date.AddDays(1);
            var garmentColumns = await GetGarmentColumnsAsync();

            // The user's ownership is discovered through the garments linked to each outfit.
            var ownerColumn = garmentColumns.Contains("user_id") ? "user_id" : "owner_user_id";

            // Group by DATE(o.created_at) so dashboard charts can show one bar/point per day.
            var sql = $@"SELECT DATE(o.created_at) AS day_key, COUNT(DISTINCT o.outfit_id) AS cnt
                         FROM {GetTableName()} o
                         INNER JOIN eitan_project12.outfit_garments og ON og.outfit_id = o.outfit_id
                         INNER JOIN eitan_project12.garments g ON g.garment_id = og.garment_id
                         INNER JOIN eitan_project12.users u ON u.user_id = g.{ownerColumn}
                         WHERE o.created_at >= @startUtc
                           AND o.created_at < @endUtcExclusive
                           AND LOWER(TRIM(COALESCE(u.role,''))) = @role
                         GROUP BY DATE(o.created_at)
                         ORDER BY day_key";
            var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>
            {
                ["startUtc"] = startUtc,
                ["endUtcExclusive"] = endUtcExclusive,
                ["role"] = normalizedRole
            });
            return ParseDailyCounts(rows);
        }

        // Saves the outfit metadata part of the outfit generation process.
        // Inserts only the columns that exist in the current outfits table.
        // Garment links are saved separately in OutfitGarmentDB through outfit_garments.
        // Project process affected: user presses save on a recommendation, then MatchingService stores the outfit.
        // FLOW_MATCH_SAVE_WEB_04 / FLOW_MATCH_SAVE_MOBILE_05:
        // OutfitDB.CreateAsync inserts the saved recommendation metadata into outfits.
        // This DB file is involved because saved outfit cards are loaded from this table; next step is OutfitGarmentDB link inserts.
        public async Task<int> CreateAsync(Outfit o)
        {
            // The seed garment is the garment that started/requested the recommendation, when there is one.
            var seedGarmentId = FirstNonEmpty(o.SeedGarmentId, FirstRequestedId(o.RequestedGarmentIds));

            // Nullable model values become DBNull so MySQL stores SQL NULL instead of an empty fake value.
            var fields = new Dictionary<string, object>
            {
                ["outfit_id"] = string.IsNullOrWhiteSpace(o.OutfitId) ? Guid.NewGuid().ToString() : o.OutfitId,
                ["label_is_compatible"] = o.LabelIsCompatible ? 1 : 0,
                ["label_source"] = string.IsNullOrWhiteSpace(o.LabelSource) ? "auto" : o.LabelSource,
                ["score"] = o.Score,
                ["outfit_style"] = (object?)o.StyleLabel ?? DBNull.Value,
                ["explanation"] = (object?)o.Explanation ?? DBNull.Value,
                ["recommended_where"] = (object?)o.RecommendedPlaces ?? DBNull.Value,
                ["seed_type"] = (object?)o.SeedType ?? DBNull.Value,
                ["seed_garment_id"] = (object?)seedGarmentId ?? DBNull.Value,
                ["created_at"] = o.CreatedAt == default ? DateTime.UtcNow : o.CreatedAt
            };

            return await InsertAsync(fields);
        }

        // Checks whether an outfit recommendation already exists before MatchingService saves it.
        // This protects the saved outfits page from repeated shirt + pants + shoes combinations.
        // Project process affected: duplicate prevention during recommendation saving.
        // FLOW_MATCH_SAVE_WEB_03 / FLOW_MATCH_SAVE_MOBILE_04:
        // OutfitDB.ExistsDuplicateAsync checks whether the same shirt/pants/shoes outfit is already saved.
        // This DB file is involved to prevent duplicate saved outfits; next step is insert or duplicate response.
        public async Task<bool> ExistsDuplicateAsync(
            string userId,
            string shirtGarmentId,
            string pantsGarmentId,
            string shoesGarmentId,
            string? seedType,
            string? seedGarmentId)
        {
            // Used by the outfit generation/save process to avoid duplicate recommendations.
            // It prevents saving the same shirt + pants + shoes set again for the same user and optional seed.
            var normalizedSeedType = (seedType ?? string.Empty).Trim().ToLowerInvariant();
            var normalizedSeedGarmentId = (seedGarmentId ?? string.Empty).Trim();
            var garmentColumns = await GetGarmentColumnsAsync();
            var ownerColumn = garmentColumns.Contains("user_id") ? "user_id" : "owner_user_id";

            // roleExpr normalizes outfit_garments.role so labels like top/sneaker still compare correctly.
            var roleExpr = BuildRoleNormalizationExpression("og");

            // The first EXISTS proves the outfit belongs to this user through at least one linked garment.
            // The next three EXISTS require the exact shirt, pants, and shoes garment ids.
            var sql = $@"SELECT o.outfit_id
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

            var parameters = new Dictionary<string, object>
            {
                // Parameters keep user/garment ids out of the SQL text and prevent SQL injection.
                ["uid"] = userId,
                ["shirt"] = shirtGarmentId,
                ["pants"] = pantsGarmentId,
                ["shoes"] = shoesGarmentId
            };

            if (!string.IsNullOrWhiteSpace(normalizedSeedType))
            {
                // Seed type is optional, so it is only part of the duplicate check when supplied.
                sql += " AND LOWER(TRIM(COALESCE(o.seed_type,''))) = @seedType";
                parameters["seedType"] = normalizedSeedType;
            }

            if (!string.IsNullOrWhiteSpace(normalizedSeedGarmentId))
            {
                // Seed garment is optional, so it is only part of the duplicate check when supplied.
                sql += " AND o.seed_garment_id = @seedGarmentId";
                parameters["seedGarmentId"] = normalizedSeedGarmentId;
            }

            sql += " LIMIT 1";
            var rows = await StingListSelectAllAsync(sql, parameters);
            return rows.Count > 0;
        }

        // Main read path for the saved outfits screen.
        // It fetches DB rows first, then applies filters that need joined facts.
        // Project process affected: saved outfits list and saved outfit filters in web and mobile API.
        // FLOW_OUTFIT_FILTER_WEB_03 / FLOW_OUTFIT_FILTER_API_03:
        // OutfitDB.GetFilteredByUserAsync applies score/style/where-to-wear/season filters.
        // This DB file is involved because outfit filtering combines outfit rows and linked garment facts; next step is UI/API response.
        public async Task<List<Outfit>> GetFilteredByUserAsync(string userId, OutfitFilterRequest? filter, int take = 200)
        {
            filter ??= new OutfitFilterRequest();

            // Normalize lowercase/list values once so the SQL and in-memory filter compare consistently.
            filter.Normalize();

            // Pull extra rows first because some filters are applied in memory after loading garment facts.
            var rawOutfits = await QueryBaseAsync(userId, filter, Math.Max(400, take * 4));

            Dictionary<string, OutfitFacts>? factsByOutfit = null;
            var needsFacts =
                filter.Seasons.Count > 0;

            // Only load linked garment facts when the user selected filters that need them.
            if (needsFacts)
            {
                // Season is stored on garments, not outfits.
                factsByOutfit = await QueryOutfitFactsByOutfitAsync(userId, rawOutfits.Select(x => x.OutfitId));
            }

            // Final filtering stays in C# because recommended places and garment facts are derived values.
            var filtered = rawOutfits
                .Where(o => MatchesOutfitFilter(o, filter, factsByOutfit))
                .OrderByDescending(o => o.CreatedAt)
                .Take(Math.Max(1, take))
                .ToList();

            return filtered;
        }

        // Builds the selectable filter values for the saved outfits page.
        // Values come from the user's existing outfits and linked garments.
        // Project process affected: the filter dropdowns/chips shown on the Outfits page.
        // FLOW_OUTFIT_FILTER_WEB_04 / FLOW_OUTFIT_FILTER_API_04:
        // OutfitDB.GetFilterOptionsAsync builds selectable outfit filter options from saved outfit data.
        // This DB file is involved so options match current results; final step is filter drawer/API response.
        public async Task<OutfitFilterOptions> GetFilterOptionsAsync(string userId, OutfitFilterRequest? filter)
        {
            filter ??= new OutfitFilterRequest();

            // Filter options should reflect the current active filter state on the Outfits page.
            filter.Normalize();

            // These options come from columns stored directly on outfits.
            var outfits = await GetFilteredByUserAsync(userId, filter, 1200);
            var options = new OutfitFilterOptions
            {
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

            // These options come from the garments linked to each outfit.
            var facts = await QueryOutfitFactsByOutfitAsync(userId, outfits.Select(o => o.OutfitId));
            options.Seasons = facts.Values
                .SelectMany(v => v.Seasons)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return options;
        }

        // Query used by the saved outfits page.
        // Projects the real schema into the Outfit model shape expected by the application.
        // The shirt/pants/shoes ids are built from outfit_garments.role.
        private async Task<List<Outfit>> QueryBaseAsync(string userId, OutfitFilterRequest filter, int take)
        {
            var garmentColumns = await GetGarmentColumnsAsync();
            var ownerColumn = garmentColumns.Contains("user_id") ? "user_id" : "owner_user_id";

            // Convert outfit_garments.role into shirt/pants/shoes buckets inside the SQL projection.
            var roleExpr = BuildRoleNormalizationExpression("og");

            // This query turns multiple outfit_garments rows into one Outfit model row.
            // MAX(CASE WHEN role = ... THEN garment_id END) picks the linked garment id for each slot.
            var sql = $@"SELECT o.outfit_id,
                                @uid AS user_id,
                                MAX(CASE WHEN {roleExpr} = 'shirt' THEN og.garment_id END) AS shirt_garment_id,
                                MAX(CASE WHEN {roleExpr} = 'pants' THEN og.garment_id END) AS pants_garment_id,
                                MAX(CASE WHEN {roleExpr} = 'shoes' THEN og.garment_id END) AS shoes_garment_id,
                                MAX(CAST(ROUND(COALESCE(o.score, 0)) AS SIGNED)) AS score,
                                1 AS rank_value,
                                MAX(o.outfit_style) AS style_label,
                                MAX(o.explanation) AS explanation,
                                MAX(o.recommended_where) AS recommended_places,
                                MAX(o.seed_type) AS seed_type,
                                MAX(o.label_is_compatible) AS label_is_compatible,
                                MAX(o.label_source) AS label_source,
                                MAX(o.seed_garment_id) AS requested_garment_ids,
                                MAX(o.created_at) AS created_at
                         FROM {GetTableName()} o
                         LEFT JOIN eitan_project12.outfit_garments og ON og.outfit_id = o.outfit_id
                         WHERE EXISTS (
                             -- Only return outfits that belong to this user through linked garments.
                             SELECT 1
                             FROM eitan_project12.outfit_garments ogu
                             INNER JOIN eitan_project12.garments gu ON gu.garment_id = ogu.garment_id
                             WHERE ogu.outfit_id = o.outfit_id
                               AND gu.{ownerColumn} = @uid
                         )
                           AND COALESCE(o.score, 0) BETWEEN @minScore AND @maxScore
                         GROUP BY o.outfit_id
                         ORDER BY MAX(o.created_at) DESC
                         LIMIT @take";

            return await SelectAllAsync(sql, new Dictionary<string, object>
            {
                ["uid"] = userId,
                ["minScore"] = filter.MinScore,
                ["maxScore"] = filter.MaxScore,
                ["take"] = Math.Max(1, take)
            });
        }

        // Loads derived facts for each outfit from the garments linked in outfit_garments.
        // This supports filtering saved outfits by season.
        private async Task<Dictionary<string, OutfitFacts>> QueryOutfitFactsByOutfitAsync(string userId, IEnumerable<string> outfitIds)
        {
            // Clean caller input because this method builds an IN list for the requested outfit ids.
            var ids = outfitIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (ids.Count == 0)
                return new Dictionary<string, OutfitFacts>(StringComparer.OrdinalIgnoreCase);

            // Some garment schemas have season and some do not, so check before selecting that column.
            var garmentColumns = await GetGarmentColumnsAsync();
            var ownerColumn = garmentColumns.Contains("user_id") ? "user_id" : "owner_user_id";
            var hasSeason = garmentColumns.Contains("season");

            var placeholders = string.Join(",", ids.Select((_, i) => $"@id{i}"));
            var parameters = new Dictionary<string, object> { ["uid"] = userId };

            // Build @id0, @id1, ... parameters for the IN query.
            for (var i = 0; i < ids.Count; i++)
                parameters[$"id{i}"] = ids[i];

            // If the column does not exist, select an empty string so the parsing code can stay simple.
            var seasonExpr = hasSeason ? "LOWER(TRIM(COALESCE(g.season,'')))" : "''";

            var sql = $@"SELECT og.outfit_id,
                                {seasonExpr} AS season
                         FROM eitan_project12.outfit_garments og
                         INNER JOIN eitan_project12.garments g ON g.garment_id = og.garment_id
                         WHERE og.outfit_id IN ({placeholders})
                           AND g.{ownerColumn} = @uid";

            var rows = await StingListSelectAllAsync(sql, parameters);
            var result = new Dictionary<string, OutfitFacts>(StringComparer.OrdinalIgnoreCase);

            // Group each linked garment's facts under its outfit id.
            foreach (var row in rows)
            {
                if (row.Length < 2)
                    continue;

                var outfitId = row[0]?.ToString();
                if (string.IsNullOrWhiteSpace(outfitId))
                    continue;

                if (!result.TryGetValue(outfitId, out var facts))
                {
                    facts = new OutfitFacts();
                    result[outfitId] = facts;
                }

                var season = (row[1]?.ToString() ?? string.Empty).Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(season))
                    facts.Seasons.Add(season);
            }

            return result;
        }

        // Applies the saved outfits page filter request to one outfit after the needed DB data has been loaded.
        private static bool MatchesOutfitFilter(
            Outfit outfit,
            OutfitFilterRequest filter,
            Dictionary<string, OutfitFacts>? factsByOutfit)
        {
            // Score, style, and recommended places are already on the Outfit model.
            if (outfit.Score < filter.MinScore || outfit.Score > filter.MaxScore)
                return false;

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
                !filter.Seasons.Any())
            {
                // No garment-fact filters were requested, so the outfit passed all direct filters.
                return true;
            }

            // If the user selected garment fact filters, an outfit must have facts loaded to match.
            if (!factsByOutfit.TryGetValue(outfit.OutfitId, out var facts))
                return false;

            if (filter.Seasons.Count > 0 && !facts.Seasons.Overlaps(filter.Seasons))
                return false;

            return true;
        }

        // Reads and caches garment columns for outfit ownership/filter queries.
        // This keeps the outfit pages working if garments uses user_id or the older owner_user_id.
        private async Task<HashSet<string>> GetGarmentColumnsAsync()
        {
            if (_garmentColumns != null)
                return _garmentColumns;

            _garmentColumns = await QueryColumnSetAsync("garments");
            return _garmentColumns;
        }

        // Asks MySQL what columns exist on a table in the eitan_project12 schema.
        // OutfitDB uses this before building queries that depend on optional garment columns.
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

        // Normalizes outfit_garments.role values into the three roles the outfit process uses:
        // shirt, pants, shoes.
        private static string BuildRoleNormalizationExpression(string alias)
        {
            var roleSource = $"LOWER(TRIM({alias}.role))";
            return GarmentTypeNormalizer.BuildSqlCaseExpression(roleSource);
        }

        // Splits recommended_where text into individual filter tokens for the saved outfits filter UI.
        private static IEnumerable<string> SplitRecommendationTokens(string? places)
        {
            if (string.IsNullOrWhiteSpace(places))
                return Enumerable.Empty<string>();

            return places
                .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => x.Trim().ToLowerInvariant())
                .Where(x => !string.IsNullOrWhiteSpace(x));
        }

        // Converts SQL numeric values into the app's 0-100 outfit score range.
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

        // Converts COUNT(*) query output into a safe int for dashboard/admin stats.
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

        // Converts grouped date/count rows into a dictionary used by dashboard charts.
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

        // Used when saving/loading outfit seed data.
        // The current outfits table stores one seed_garment_id, but this also handles older comma-separated values.
        private static string? FirstRequestedId(string? requestedGarmentIds)
        {
            if (string.IsNullOrWhiteSpace(requestedGarmentIds))
                return null;

            return requestedGarmentIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();
        }

        // Used by outfit saving to choose the best available seed garment id.
        private static string? FirstNonEmpty(params string?[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return null;
        }

        // Small container for saved-outfit filter facts collected from linked garments.
        private sealed class OutfitFacts
        {
            public HashSet<string> Seasons { get; } = new(StringComparer.OrdinalIgnoreCase);
        }
    }
}
