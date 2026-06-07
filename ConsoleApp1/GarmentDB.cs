
// SEARCH INDEX
// DATABASE, GARMENT, CLOSET, IMAGE, UPLOAD, FILTER, SEARCH, ADD, REMOVE, COUNT, SHA256, DUPLICATE
//
// Topic: GARMENT DATABASE
// Purpose: Stores and retrieves closet garment rows, including image bytes and AI-extracted features.
// Search keywords: DATABASE GARMENT CLOSET IMAGE UPLOAD FILTER SEARCH ADD REMOVE COUNT SHA256 DUPLICATE
// When to use it: Show this when explaining upload, closet display, image loading, filters, or matcher input.
// Important notes: Image data is stored on the garments table; there is no separate garment_images table in current flow.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Models;
using MySql.Data.MySqlClient;

namespace DBL
{
    // SECTION: GARMENT DATABASE CLOSET IMAGE
    // Topic: GarmentDB class
    // Purpose: Table-specific DB logic for eitan_project12.garments.
    // Search keywords: DATABASE GARMENT CLOSET IMAGE FILTER ADD REMOVE SEARCH
    // When to use it: Use for loading a user's closet, saving uploads, filtering garments, and serving image bytes.
    // Important notes: The code keeps some legacy-column fallbacks so the project survives schema differences.
    // Role in project:
    // Database access class for the closet garment records.
    // This affects upload, closet display, admin closet view, mobile closet API, filters, and matcher input data.
    public class GarmentDB : BaseDB<Garment>
    {
        // LastError is read by pages/API after a failed DB action so the UI can show a useful message.
        public string? LastError { get; private set; }

        // The project used owner_user_id before the merge; new schema uses user_id.
        // These constants let the same code work safely during/after the schema change.
        private const string PreferredUserColumn = "user_id";
        private const string LegacyUserColumn = "owner_user_id";
        private HashSet<string>? _garmentColumns;

        // BaseDB needs the table name and primary key to build generic insert/delete SQL.
        protected override string GetPrimaryKeyName() => "garment_id";
        protected override string GetTableName() => "eitan_project12.garments";

        // Converts one SQL result row into the Garment model used by pages, services, and APIs.
        // The long branch is the current merged garments schema; the short branch is a defensive legacy fallback.
        protected override Task<Garment> CreateModelAsync(object[] row)
        {
            if (row.Length >= 20)
            {
                return Task.FromResult(new Garment
                {
                    GarmentId = row[0]?.ToString() ?? string.Empty,
                    OwnerUserId = row[1]?.ToString() ?? string.Empty,
                    Type = row[2]?.ToString() ?? string.Empty,
                    Color = row[3]?.ToString(),
                    ColorSecondary = row[4]?.ToString(),
                    Pattern = row[5]?.ToString(),
                    StyleCategory = row[6]?.ToString(),
                    Season = row[7]?.ToString(),
                    Occasion = row[8]?.ToString(),
                    FormalityLevel = TryParseNullableInt(row[9]),
                    StyleTags = row[10]?.ToString(),
                    Fit = row[11]?.ToString(),
                    Material = row[12]?.ToString(),
                    Sleeve = row[13]?.ToString(),
                    Length = row[14]?.ToString(),
                    Brand = row[15]?.ToString(),
                    FeatureJson = row[16]?.ToString(),
                    Sha256 = row[17]?.ToString() ?? string.Empty,
                    ImageMimeType = string.IsNullOrWhiteSpace(row[18]?.ToString())
                        ? "image/jpeg"
                        : row[18]?.ToString() ?? "image/jpeg",
                    CreatedAt = ParseDate(row[19]) ?? DateTime.UtcNow
                });
            }

            return Task.FromResult(new Garment
            {
                GarmentId = row.Length > 0 ? row[0]?.ToString() ?? string.Empty : string.Empty,
                Type = row.Length > 1 ? row[1]?.ToString() ?? string.Empty : string.Empty,
                Color = row.Length > 2 ? row[2]?.ToString() : null,
                Brand = row.Length > 3 ? row[3]?.ToString() : null,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Builds the SELECT list in one place so every garment query returns fields in the same order.
        // It also handles optional feature_json so older databases do not break closet loading.
        private static string SelectColumns(string userColumn, IReadOnlySet<string> columns)
        {
            var featureJsonColumn = columns.Contains("feature_json")
                ? "feature_json"
                : "NULL";

            return
            $@"garment_id,
               {userColumn} AS user_id,
               type,
               color,
               color_secondary,
               pattern,
               style_category,
               season,
               occasion,
               formality_level,
               style_tags,
               fit,
               material,
               sleeve,
               length,
               brand,
               {featureJsonColumn} AS feature_json,
               sha256,
               image_mime_type,
               created_at";
        }

        // MySQL 1054 means a column is missing; CreateAsync uses it to try a simpler insert shape.
        private static bool IsUnknownColumn(MySqlException ex) => ex.Number == 1054;

        private static DateTime? ParseDate(object? value)
        {
            if (value == null) return null;
            return DateTime.TryParse(value.ToString(), out var dt) ? dt : null;
        }

        // Converts nullable DB values into nullable integers for fields like formality_level.
        private static int? TryParseNullableInt(object? value)
        {
            if (value == null) return null;
            var text = value.ToString();
            return int.TryParse(text, out var parsed) ? parsed : null;
        }

        // Chooses the active owner column for the installed database schema.
        private async Task<string> GetUserColumnAsync()
        {
            var columns = await GetGarmentColumnsAsync();
            return columns.Contains(PreferredUserColumn) ? PreferredUserColumn : LegacyUserColumn;
        }

        // Reads garment table column names once and caches them.
        // This protects the project while SQL is being simplified/merged.
        private async Task<HashSet<string>> GetGarmentColumnsAsync()
        {
            if (_garmentColumns != null)
                return _garmentColumns;

            var sql = @"SELECT LOWER(column_name)
                        FROM information_schema.columns
                        WHERE table_schema = 'eitan_project12'
                          AND table_name = 'garments'";
            var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>());
            _garmentColumns = rows
                .Select(r => r.Length > 0 ? r[0]?.ToString() ?? string.Empty : string.Empty)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim().ToLowerInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return _garmentColumns;
        }

        // Main closet load function.
        // Used by Closet page, admin views, mobile API, and MatchingService to fetch a user's garments.
        // FLOW_CLOSET_VIEW_WEB_03: GarmentDB.GetByUserAsync loads closet rows for the signed-in web user.
        // This DB file is involved because garments are the closet source of truth; next step is Closet.razor rendering.
        // FLOW_CLOSET_VIEW_MOBILE_05: GarmentDB.GetByUserAsync loads closet rows for the MAUI user or admin-selected customer.
        // This DB file is involved because the mobile API returns these rows as JSON; next step is Program.cs returning them.
        // FLOW_MATCH_RUN_WEB_04: GarmentDB.GetByUserAsync loads the user's closet rows for the web matcher.
        // This DB file is involved because MatchingService needs authoritative garments; next step is CanMatchAsync minimum validation.
        // FLOW_MATCH_RUN_MOBILE_06: GarmentDB.GetByUserAsync loads the user's closet rows for the MAUI matcher.
        // This DB file is involved because MatchingService needs authoritative garments; next step is CanMatchAsync minimum validation.
        // FLOW_DELETE_ACCOUNT_WEB_04 / FLOW_DELETE_ACCOUNT_MOBILE_05:
        // GarmentDB.GetByUserAsync loads garments that must be deleted during account cascade cleanup.
        public async Task<List<Garment>> GetByUserAsync(string userId)
        {
            LastError = null;

            try
            {
                var userColumn = await GetUserColumnAsync();
                var columns = await GetGarmentColumnsAsync();
                var sql = $"SELECT {SelectColumns(userColumn, columns)} FROM {GetTableName()} WHERE {userColumn} = @uid ORDER BY created_at DESC";
                return await SelectAllAsync(sql, new Dictionary<string, object> { ["uid"] = userId });
            }
            catch (MySqlException ex)
            {
                LastError = $"MySQL {ex.Number}: {ex.Message}";
                return new List<Garment>();
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return new List<Garment>();
            }
        }

        // Backward-compatible alias for older code that still calls GetByUserIdAsync.
        public Task<List<Garment>> GetByUserIdAsync(string userId) => GetByUserAsync(userId);

        // Loads one garment by id for delete checks, admin actions, and mobile endpoints.
        public async Task<Garment?> GetByIdAsync(string garmentId)
        {
            LastError = null;

            try
            {
                var userColumn = await GetUserColumnAsync();
                return await GetByIdUsingUserColumnAsync(garmentId, userColumn);
            }
            catch (MySqlException ex)
            {
                LastError = $"MySQL {ex.Number}: {ex.Message}";
                return null;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return null;
            }
        }

        // Loads one garment after the active owner column has already been resolved.
        private async Task<Garment?> GetByIdUsingUserColumnAsync(string garmentId, string userColumn)
        {
            var columns = await GetGarmentColumnsAsync();
            var sql = $"SELECT {SelectColumns(userColumn, columns)} FROM {GetTableName()} WHERE garment_id = @id";
            var list = await SelectAllAsync(sql, new Dictionary<string, object> { ["id"] = garmentId });
            return list.FirstOrDefault();
        }

        // Serves the stored garment image bytes through /media/garments/by-garment/{id}.
        // This is why closet cards and outfit piece previews can render images.
        // FLOW_GARMENT_IMAGE_SERVE_03: GarmentDB.GetImageByGarmentIdAsync loads stored image bytes and mime type.
        // This DB file is involved because images are stored on the garment row; final step is Program.cs returning the file response.
        public async Task<(byte[] Bytes, string MimeType)?> GetImageByGarmentIdAsync(string garmentId)
        {
            LastError = null;

            var sql = $@"SELECT image_bytes, image_mime_type
                         FROM {GetTableName()}
                         WHERE garment_id = @id";
            var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>
            {
                ["id"] = garmentId
            });

            if (rows.Count == 0 || rows[0].Length < 2 || rows[0][0] == null || rows[0][0] == DBNull.Value)
                return null;

            var bytes = (byte[])rows[0][0];
            var mime = rows[0][1]?.ToString();
            return (bytes, string.IsNullOrWhiteSpace(mime) ? "application/octet-stream" : mime);
        }

        // Inserts a new garment after web/mobile upload and AI feature extraction.
        // Strategies let the insert still work if the local SQL table has fewer optional columns.
        // FLOW_GARMENT_UPLOAD_WEB_05 / FLOW_GARMENT_UPLOAD_MOBILE_06:
        // GarmentDB.CreateAsync inserts analyzed garment fields, image bytes, mime type, and sha256.
        // This DB file is involved because upload ends in the garments table; next step is UI/API reload success.
        public async Task<int> CreateAsync(Garment g)
        {
            LastError = null;
            var userColumn = await GetUserColumnAsync();

            var strategies = new[]
            {
                new { IncludeFeatureJson = true, IncludeExtended = true },
                new { IncludeFeatureJson = false, IncludeExtended = true },
                new { IncludeFeatureJson = true, IncludeExtended = false },
                new { IncludeFeatureJson = false, IncludeExtended = false }
            };

            foreach (var strategy in strategies)
            {
                try
                {
                    var fields = BuildFieldDictionary(g, userColumn, strategy.IncludeFeatureJson, strategy.IncludeExtended);
                    var inserted = await InsertAsync(fields);
                    if (inserted > 0)
                        return inserted;
                }
                catch (MySqlException ex) when (IsUnknownColumn(ex))
                {
                    LastError = $"MySQL {ex.Number}: {ex.Message}";
                    continue;
                }
                catch (MySqlException ex)
                {
                    LastError = $"MySQL {ex.Number}: {ex.Message}";
                    return 0;
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    return 0;
                }
            }

            return 0;
        }

        // Converts the Garment model into DB column values.
        // DBNull.Value is required for nullable SQL columns.
        private static Dictionary<string, object> BuildFieldDictionary(
            Garment g,
            string userColumn,
            bool includeFeatureJson,
            bool includeExtended)
        {
            var values = new Dictionary<string, object>
            {
                ["garment_id"] = g.GarmentId,
                [userColumn] = g.OwnerUserId,
                ["type"] = g.Type,
                ["color"] = (object?)g.Color ?? DBNull.Value,
                ["fit"] = (object?)g.Fit ?? DBNull.Value,
                ["material"] = (object?)g.Material ?? DBNull.Value,
                ["sleeve"] = (object?)g.Sleeve ?? DBNull.Value,
                ["length"] = (object?)g.Length ?? DBNull.Value,
                ["brand"] = (object?)g.Brand ?? DBNull.Value,
                ["image_bytes"] = (object?)g.ImageBytes ?? DBNull.Value,
                ["image_mime_type"] = string.IsNullOrWhiteSpace(g.ImageMimeType) ? "image/jpeg" : g.ImageMimeType,
                ["sha256"] = g.Sha256,
                ["created_at"] = g.CreatedAt == default ? DateTime.UtcNow : g.CreatedAt
            };

            if (includeExtended)
            {
                values["color_secondary"] = (object?)g.ColorSecondary ?? DBNull.Value;
                values["pattern"] = (object?)g.Pattern ?? DBNull.Value;
                values["style_category"] = (object?)g.StyleCategory ?? DBNull.Value;
                values["season"] = (object?)g.Season ?? DBNull.Value;
                values["occasion"] = (object?)g.Occasion ?? DBNull.Value;
                values["formality_level"] = (object?)g.FormalityLevel ?? DBNull.Value;
                values["style_tags"] = (object?)g.StyleTags ?? DBNull.Value;
            }

            if (includeFeatureJson)
                values["feature_json"] = (object?)g.FeatureJson ?? DBNull.Value;

            return values;
        }

        // Deletes one garment from the closet.
        // Used by customer closet delete and admin customer-management delete.
        // FLOW_DELETE_GARMENT_WEB_04 / FLOW_DELETE_GARMENT_MOBILE_05 / FLOW_DELETE_ACCOUNT_WEB_05 / FLOW_DELETE_ACCOUNT_MOBILE_05:
        // GarmentDB.DeleteGarmentAsync removes one garment row after ownership/cascade logic chose it.
        // This DB file is involved because the closet table is updated here; next step is refresh or continuing cascade.
        public async Task<int> DeleteGarmentAsync(string garmentId)
        {
            LastError = null;

            try
            {
                var parameters = new Dictionary<string, object>
                {
                    ["garment_id"] = garmentId
                };

                return await DeleteAsync(parameters);
            }
            catch (MySqlException ex)
            {
                LastError = $"MySQL {ex.Number}: {ex.Message}";
                return 0;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return 0;
            }
        }

        // Admin dashboard statistic: total garments belonging to users with a given role.
        public async Task<int> CountByUserRoleAsync(string role)
        {
            LastError = null;
            var normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();

            try
            {
                var userColumn = await GetUserColumnAsync();
                return await CountByUserRoleInternalAsync(normalizedRole, userColumn);
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return 0;
            }
        }

        // Admin dashboard chart: garments created per day for a role, usually customers.
        public async Task<Dictionary<DateTime, int>> GetDailyCreatedCountsByUserRoleAsync(int days, string role)
        {
            LastError = null;
            var normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();
            var safeDays = Math.Clamp(days, 1, 365);
            var startUtc = DateTime.UtcNow.Date.AddDays(-(safeDays - 1));
            var endUtcExclusive = DateTime.UtcNow.Date.AddDays(1);

            try
            {
                var userColumn = await GetUserColumnAsync();
                return await GetDailyCreatedCountsByUserRoleInternalAsync(normalizedRole, startUtc, endUtcExclusive, userColumn);
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return new Dictionary<DateTime, int>();
            }
        }

        // Matcher readiness helper: counts how many shirts, pants, and shoes a user has.
        // Current MatchingService usually validates from the already-loaded garment list, so this is a support helper,
        // not the main numbered recommendation path.
        public async Task<Dictionary<string, int>> CountByTypeAsync(string userId)
        {
            LastError = null;
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var userColumn = await GetUserColumnAsync();
                var sql = $@"SELECT type, COUNT(*) AS cnt
                             FROM {GetTableName()}
                             WHERE {userColumn} = @uid
                             GROUP BY type";
                var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object> { ["uid"] = userId });
                foreach (var row in rows)
                {
                    var type = GarmentTypeNormalizer.Normalize(row[0]?.ToString());
                    if (int.TryParse(row[1]?.ToString(), out var c))
                    {
                        if (result.ContainsKey(type)) result[type] += c;
                        else result[type] = c;
                    }
                }
                LogCounts(userId, result);
                return result;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return result;
            }
        }

        // Debug-only count logging used while checking matcher closet readiness.
        private void LogCounts(string userId, Dictionary<string, int> counts)
        {
            try
            {
                var summary = string.Join(",", counts.Select(kv => $"{kv.Key}:{kv.Value}"));
                System.Diagnostics.Debug.WriteLine($"[CountByType] user={userId} counts={summary}");
            }
            catch
            {
            }
        }

        // Runs the role-based garment count after the active owner column is known.
        private async Task<int> CountByUserRoleInternalAsync(string normalizedRole, string ownerColumn)
        {
            var sql = $@"SELECT COUNT(*)
                         FROM {GetTableName()} g
                         INNER JOIN eitan_project12.users u ON u.user_id = g.{ownerColumn}
                         WHERE LOWER(TRIM(COALESCE(u.role,''))) = @role";
            var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>
            {
                ["role"] = normalizedRole
            });
            return ParseCount(rows);
        }

        private async Task<Dictionary<DateTime, int>> GetDailyCreatedCountsByUserRoleInternalAsync(
            string normalizedRole,
            DateTime startUtc,
            DateTime endUtcExclusive,
            string ownerColumn)
        {
            var sql = $@"SELECT DATE(g.created_at) AS day_key, COUNT(*) AS cnt
                         FROM {GetTableName()} g
                         INNER JOIN eitan_project12.users u ON u.user_id = g.{ownerColumn}
                         WHERE g.created_at >= @startUtc
                           AND g.created_at < @endUtcExclusive
                           AND LOWER(TRIM(COALESCE(u.role,''))) = @role
                         GROUP BY DATE(g.created_at)
                         ORDER BY day_key";
            var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>
            {
                ["startUtc"] = startUtc,
                ["endUtcExclusive"] = endUtcExclusive,
                ["role"] = normalizedRole
            });
            return ParseDailyCounts(rows);
        }

        // Safely converts a COUNT(*) result row into a non-negative int.
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

        // Converts grouped SQL day/count rows into the admin dashboard chart dictionary.
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

        // MatchingService uses this to get candidate shirts/pants/shoes while excluding selected seed items.
        // FLOW_MATCH_RUN_WEB_06 / FLOW_MATCH_RUN_MOBILE_08:
        // GarmentDB.GetByUserAndTypeAsync loads candidate pools by type while excluding selected seed garments.
        // This DB file is involved because combinations must use real garments; next step is MatchingService builds combinations.
        public async Task<List<Garment>> GetByUserAndTypeAsync(string userId, string type, IEnumerable<string>? excludeIds = null)
        {
            LastError = null;
            var exList = excludeIds?.ToList() ?? new List<string>();

            try
            {
                var userColumn = await GetUserColumnAsync();
                var columns = await GetGarmentColumnsAsync();
                var sql = $@"SELECT {SelectColumns(userColumn, columns)} FROM {GetTableName()}
                             WHERE {userColumn} = @uid AND type = @t";
                if (exList.Count > 0)
                {
                    var placeholders = string.Join(",", exList.Select((_, i) => $"@ex{i}"));
                    sql += $" AND garment_id NOT IN ({placeholders})";
                }
                sql += " ORDER BY created_at DESC";

                var parameters = new Dictionary<string, object>
                {
                    ["uid"] = userId,
                    ["t"] = type
                };
                for (int i = 0; i < exList.Count; i++)
                    parameters[$"ex{i}"] = exList[i];

                return await SelectAllAsync(sql, parameters);
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return new List<Garment>();
            }
        }

        // Closet filter result query.
        // Used by web closet filters and internal/mobile closet APIs.
        // FLOW_CLOSET_FILTER_WEB_03 / FLOW_CLOSET_FILTER_API_03:
        // GarmentDB.GetFilteredByUserAsync applies category/color/brand/season/occasion filters in SQL.
        // This DB file is involved because filtering belongs next to the data query; next step is UI/API returns filtered garments.
        public async Task<List<Garment>> GetFilteredByUserAsync(string userId, GarmentFilterRequest? filter, int take = 600)
        {
            LastError = null;
            filter ??= new GarmentFilterRequest();
            filter.Normalize();

            try
            {
                var userColumn = await GetUserColumnAsync();
                var columns = await GetGarmentColumnsAsync();
                return await GetFilteredByUserColumnAsync(userColumn, columns, userId, filter, take);
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return new List<Garment>();
            }
        }

        // Builds available filter choices after the current filter state is applied.
        // This keeps drawer options relevant instead of showing values that would return no garments.
        // FLOW_CLOSET_FILTER_WEB_04 / FLOW_CLOSET_FILTER_API_04:
        // GarmentDB.GetFilterOptionsAsync builds selectable filter options, including unknown values.
        // This DB file is involved because options must match current DB data; final step is filter drawer/API response.
        public async Task<GarmentFilterOptions> GetFilterOptionsAsync(string userId, GarmentFilterRequest? filter)
        {
            LastError = null;
            filter ??= new GarmentFilterRequest();
            filter.Normalize();

            try
            {
                var userColumn = await GetUserColumnAsync();
                return await GetFilterOptionsByUserColumnAsync(userColumn, userId, filter);
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return new GarmentFilterOptions();
            }
        }

        // Actual filtered SELECT once the correct owner column and table columns are known.
        private async Task<List<Garment>> GetFilteredByUserColumnAsync(
            string userColumn,
            IReadOnlySet<string> columns,
            string userId,
            GarmentFilterRequest filter,
            int take)
        {
            var parameters = new Dictionary<string, object>
            {
                ["uid"] = userId,
                ["take"] = Math.Max(1, take)
            };

            var where = BuildGarmentWhereClause(filter, parameters, null, userColumn);
            var sql = $@"SELECT {SelectColumns(userColumn, columns)} FROM {GetTableName()}
                         {where}
                         ORDER BY created_at DESC
                         LIMIT @take";
            return await SelectAllAsync(sql, parameters);
        }

        // Queries all closet filter option groups for the current user.
        private async Task<GarmentFilterOptions> GetFilterOptionsByUserColumnAsync(
            string userColumn,
            string userId,
            GarmentFilterRequest filter)
        {
            var options = new GarmentFilterOptions();

            options.Categories = await QueryDistinctCategoryOptionsAsync(userColumn, userId, filter);
            options.Seasons = await QueryDistinctColumnOptionsAsync(userColumn, userId, filter, "season", "seasons");
            options.Occasions = await QueryDistinctColumnOptionsAsync(userColumn, userId, filter, "occasion", "occasions");
            options.Brands = await QueryDistinctColumnOptionsAsync(userColumn, userId, filter, "brand", "brands");
            options.Colors = await QueryDistinctColorOptionsAsync(userColumn, userId, filter);

            return options;
        }

        // Generic helper for filters backed by one column, such as season, material, brand, fit, or pattern.
        private async Task<List<string>> QueryDistinctColumnOptionsAsync(
            string userColumn,
            string userId,
            GarmentFilterRequest filter,
            string column,
            string excludedFilterKey)
        {
            var parameters = new Dictionary<string, object>
            {
                ["uid"] = userId
            };

            var where = BuildGarmentWhereClause(filter, parameters, excludedFilterKey, userColumn);
            var sql = $@"SELECT DISTINCT COALESCE(NULLIF(LOWER(TRIM({column})), ''), 'unknown') AS value
                         FROM {GetTableName()}
                         {where}";
            var rows = await StingListSelectAllAsync(sql, parameters);
            return rows
                .Select(r => r.Length > 0 ? r[0]?.ToString() ?? string.Empty : string.Empty)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => string.Equals(v, "unknown", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
                .ThenBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // Category options are normalized through GarmentTypeNormalizer so aliases become shirt/pants/shoes.
        private async Task<List<string>> QueryDistinctCategoryOptionsAsync(
            string userColumn,
            string userId,
            GarmentFilterRequest filter)
        {
            var parameters = new Dictionary<string, object>
            {
                ["uid"] = userId
            };

            var where = BuildGarmentWhereClause(filter, parameters, "categories", userColumn);
            var sql = $@"SELECT DISTINCT COALESCE(NULLIF(LOWER(TRIM(type)), ''), 'unknown') AS value
                         FROM {GetTableName()}
                         {where}";
            var rows = await StingListSelectAllAsync(sql, parameters);

            var mapped = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows)
            {
                var raw = row.Length > 0 ? row[0]?.ToString() : null;
                if (string.Equals(raw, "unknown", StringComparison.OrdinalIgnoreCase))
                {
                    mapped.Add("unknown");
                    continue;
                }

                var normalized = GarmentTypeNormalizer.Normalize(raw);
                if (!string.IsNullOrWhiteSpace(normalized))
                    mapped.Add(normalized);
            }

            return mapped
                .OrderBy(v => string.Equals(v, "unknown", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
                .ThenBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // Color options include both primary and secondary color columns.
        private async Task<List<string>> QueryDistinctColorOptionsAsync(
            string userColumn,
            string userId,
            GarmentFilterRequest filter)
        {
            var parameters = new Dictionary<string, object>
            {
                ["uid"] = userId
            };

            var where = BuildGarmentWhereClause(filter, parameters, "colors", userColumn);
            var sql = $@"
                SELECT DISTINCT value
                FROM (
                    SELECT LOWER(TRIM(color)) AS value
                    FROM {GetTableName()}
                    {where}
                      AND color IS NOT NULL
                      AND TRIM(color) <> ''
                    UNION
                    SELECT LOWER(TRIM(color_secondary)) AS value
                    FROM {GetTableName()}
                    {where}
                      AND color_secondary IS NOT NULL
                      AND TRIM(color_secondary) <> ''
                    UNION
                    SELECT 'unknown' AS value
                    FROM {GetTableName()}
                    {where}
                      AND (color IS NULL OR TRIM(color) = '')
                      AND (color_secondary IS NULL OR TRIM(color_secondary) = '')
                ) c
                WHERE value IS NOT NULL AND TRIM(value) <> ''";

            var rows = await StingListSelectAllAsync(sql, parameters);
            return rows
                .Select(r => r.Length > 0 ? r[0]?.ToString() ?? string.Empty : string.Empty)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => string.Equals(v, "unknown", StringComparison.OrdinalIgnoreCase) ? 1 : 0)
                .ThenBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // Style tags are stored as JSON/CSV text, so this parses all saved tags into distinct filter options.
        private async Task<List<string>> QueryDistinctTagOptionsAsync(
            string userColumn,
            string userId,
            GarmentFilterRequest filter)
        {
            var parameters = new Dictionary<string, object>
            {
                ["uid"] = userId
            };

            var where = BuildGarmentWhereClause(filter, parameters, "tags", userColumn);
            var sql = $@"SELECT style_tags
                         FROM {GetTableName()}
                         {where}
                           AND style_tags IS NOT NULL
                           AND TRIM(style_tags) <> ''";

            var rows = await StingListSelectAllAsync(sql, parameters);
            var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                var raw = row.Length > 0 ? row[0]?.ToString() : null;
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                foreach (var tag in ParseStyleTags(raw))
                    tags.Add(tag);
            }

            return tags.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
        }

        // Supports both JSON array tags and older comma-separated tags.
        private static List<string> ParseStyleTags(string raw)
        {
            var result = new List<string>();

            try
            {
                var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in doc.RootElement.EnumerateArray())
                    {
                        var value = item.GetString();
                        if (!string.IsNullOrWhiteSpace(value))
                            result.Add(value.Trim().ToLowerInvariant());
                    }
                    return result;
                }
            }
            catch
            {
            }

            foreach (var item in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!string.IsNullOrWhiteSpace(item))
                    result.Add(item.Trim().Trim('"', '\'').ToLowerInvariant());
            }

            return result;
        }

        // Shared WHERE builder for garment results and filter option queries.
        // excludedFilterKey lets one filter group compute its options without filtering itself out.
        private static string BuildGarmentWhereClause(
            GarmentFilterRequest filter,
            Dictionary<string, object> parameters,
            string? excludedFilterKey = null,
            string userColumn = PreferredUserColumn)
        {
            var clauses = new List<string>
            {
                $"{userColumn} = @uid"
            };

            if (!string.Equals(excludedFilterKey, "categories", StringComparison.OrdinalIgnoreCase))
                AddCategoryClause(clauses, parameters, filter.Categories);

            if (!string.Equals(excludedFilterKey, "colors", StringComparison.OrdinalIgnoreCase) && filter.Colors.Count > 0)
                AddColorClause(clauses, parameters, filter.Colors);

            if (!string.Equals(excludedFilterKey, "seasons", StringComparison.OrdinalIgnoreCase))
                AddInClause(clauses, parameters, "season", filter.Seasons, "sea");

            if (!string.Equals(excludedFilterKey, "occasions", StringComparison.OrdinalIgnoreCase))
                AddInClause(clauses, parameters, "occasion", filter.Occasions, "occ");

            if (!string.Equals(excludedFilterKey, "brands", StringComparison.OrdinalIgnoreCase))
                AddInClause(clauses, parameters, "brand", filter.Brands, "br");

            return $"WHERE {string.Join(" AND ", clauses)}";
        }

        // Adds a parameterized IN clause for simple multi-select filters.
        private static void AddInClause(
            List<string> clauses,
            Dictionary<string, object> parameters,
            string column,
            List<string> values,
            string paramPrefix)
        {
            if (values.Count == 0)
                return;

            var normalizedValues = values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalizedValues.Count == 0)
                return;

            var wantsUnknown = normalizedValues.RemoveAll(v => string.Equals(v, "unknown", StringComparison.OrdinalIgnoreCase)) > 0;
            var predicates = new List<string>();
            var placeholders = new List<string>();
            for (var i = 0; i < normalizedValues.Count; i++)
            {
                var name = $"{paramPrefix}{i}";
                placeholders.Add($"@{name}");
                parameters[name] = normalizedValues[i];
            }

            if (placeholders.Count > 0)
                predicates.Add($"LOWER(TRIM({column})) IN ({string.Join(",", placeholders)})");

            if (wantsUnknown)
                predicates.Add($"({column} IS NULL OR TRIM({column}) = '')");

            if (predicates.Count > 0)
                clauses.Add($"({string.Join(" OR ", predicates)})");
        }

        // Adds shirt/pants/shoes matching, including common aliases produced by image analysis.
        private static void AddCategoryClause(
            List<string> clauses,
            Dictionary<string, object> parameters,
            List<string> categories)
        {
            if (categories.Count == 0)
                return;

            var wantsUnknown = categories.Any(x => string.Equals((x ?? string.Empty).Trim(), "unknown", StringComparison.OrdinalIgnoreCase));
            var normalized = categories
                .Select(GarmentTypeNormalizer.Normalize)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalized.Count == 0 && !wantsUnknown)
                return;

            var predicates = new List<string>();
            for (var i = 0; i < normalized.Count; i++)
            {
                var value = normalized[i];
                var exact = $"catExact{i}";
                parameters[exact] = value;

                if (string.Equals(value, "shirt", StringComparison.OrdinalIgnoreCase))
                {
                    predicates.Add($"(LOWER(TRIM(type)) = @{exact} OR LOWER(TRIM(type)) LIKE @catLike{i}a OR LOWER(TRIM(type)) LIKE @catLike{i}b)");
                    parameters[$"catLike{i}a"] = "%tee%";
                    parameters[$"catLike{i}b"] = "%top%";
                }
                else if (string.Equals(value, "pants", StringComparison.OrdinalIgnoreCase))
                {
                    predicates.Add($"(LOWER(TRIM(type)) = @{exact} OR LOWER(TRIM(type)) LIKE @catLike{i}a OR LOWER(TRIM(type)) LIKE @catLike{i}b)");
                    parameters[$"catLike{i}a"] = "%trouser%";
                    parameters[$"catLike{i}b"] = "%jean%";
                }
                else if (string.Equals(value, "shoes", StringComparison.OrdinalIgnoreCase))
                {
                    predicates.Add($"(LOWER(TRIM(type)) = @{exact} OR LOWER(TRIM(type)) LIKE @catLike{i}a OR LOWER(TRIM(type)) LIKE @catLike{i}b)");
                    parameters[$"catLike{i}a"] = "%sneaker%";
                    parameters[$"catLike{i}b"] = "%boot%";
                }
                else
                {
                    predicates.Add($"LOWER(TRIM(type)) = @{exact}");
                }
            }

            if (wantsUnknown)
                predicates.Add("(type IS NULL OR TRIM(type) = '')");

            if (predicates.Count > 0)
                clauses.Add($"({string.Join(" OR ", predicates)})");
        }

        // Adds color matching across both primary and secondary color fields.
        private static void AddColorClause(
            List<string> clauses,
            Dictionary<string, object> parameters,
            List<string> values)
        {
            var normalizedValues = values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalizedValues.Count == 0)
                return;

            var wantsUnknown = normalizedValues.RemoveAll(v => string.Equals(v, "unknown", StringComparison.OrdinalIgnoreCase)) > 0;
            var predicates = new List<string>();
            var placeholders = new List<string>();
            for (var i = 0; i < normalizedValues.Count; i++)
            {
                var name = $"col{i}";
                placeholders.Add($"@{name}");
                parameters[name] = normalizedValues[i];
            }

            if (placeholders.Count > 0)
            {
                var inList = string.Join(",", placeholders);
                predicates.Add($"(LOWER(TRIM(color)) IN ({inList}) OR LOWER(TRIM(color_secondary)) IN ({inList}))");
            }

            if (wantsUnknown)
            {
                predicates.Add(@"((color IS NULL OR TRIM(color) = '')
                                  AND (color_secondary IS NULL OR TRIM(color_secondary) = ''))");
            }

            if (predicates.Count > 0)
                clauses.Add($"({string.Join(" OR ", predicates)})");
        }

        // Adds tag filtering against the JSON-like saved style_tags text.
        private static void AddTagLikeClause(
            List<string> clauses,
            Dictionary<string, object> parameters,
            List<string> tags)
        {
            var builder = new StringBuilder();
            builder.Append('(');

            for (var i = 0; i < tags.Count; i++)
            {
                var name = $"tag{i}";
                if (i > 0)
                    builder.Append(" OR ");

                parameters[name] = tags[i];
                builder.Append($"LOWER(COALESCE(style_tags,'')) LIKE CONCAT('%\"', @{name}, '\"%')");
            }

            builder.Append(')');
            clauses.Add(builder.ToString());
        }
    }
}
