// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
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
    public class GarmentDB : BaseDB<Garment>
    {
        public string? LastError { get; private set; }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        protected override string GetPrimaryKeyName() => "garment_id";
        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        protected override string GetTableName() => "eitan_project12.garments";

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        protected override Task<Garment> CreateModelAsync(object[] row)
        {
            // Live schema (without feature_json column):
            // 0 garment_id
            // 1 owner_user_id/user_id
            // 2 type
            // 3 color
            // 4 color_secondary
            // 5 fit
            // 6 material
            // 7 pattern
            // 8 sleeve
            // 9 length
            // 10 brand
            // 11 style_category
            // 12 season
            // 13 occasion
            // 14 formality_level
            // 15 style_tags
            // 16 image_url
            // 17 garment_hash
            // 18 created_at
            // 19 updated_at
            if (row.Length == 20)
            {
                return Task.FromResult(new Garment
                {
                    GarmentId = row[0]?.ToString() ?? string.Empty,
                    OwnerUserId = row[1]?.ToString() ?? string.Empty,
                    Type = row[2]?.ToString() ?? string.Empty,
                    Color = row[3]?.ToString(),
                    ColorSecondary = row[4]?.ToString(),
                    Fit = row[5]?.ToString(),
                    Material = row[6]?.ToString(),
                    Pattern = row[7]?.ToString(),
                    Sleeve = row[8]?.ToString(),
                    Length = row[9]?.ToString(),
                    Brand = row[10]?.ToString(),
                    StyleCategory = row[11]?.ToString(),
                    Season = row[12]?.ToString(),
                    Occasion = row[13]?.ToString(),
                    FormalityLevel = TryParseNullableInt(row[14]),
                    StyleTags = row[15]?.ToString(),
                    ImageUrl = row[16]?.ToString(),
                    GarmentHash = row[17]?.ToString() ?? string.Empty,
                    CreatedAt = ParseDate(row[18]) ?? DateTime.UtcNow,
                    UpdatedAt = ParseDate(row[19])
                });
            }

            // New schema with extended feature columns.
            // 0 garment_id
            // 1 owner_user_id/user_id
            // 2 type
            // 3 color
            // 4 color_secondary
            // 5 pattern
            // 6 style_category
            // 7 season
            // 8 occasion
            // 9 formality_level
            // 10 style_tags
            // 11 fit
            // 12 material
            // 13 sleeve
            // 14 length
            // 15 brand
            // 16 image_url
            // 17 feature_json
            // 18 garment_hash
            // 19 created_at
            // 20 updated_at
            if (row.Length >= 21)
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
                    ImageUrl = row[16]?.ToString(),
                    FeatureJson = row[17]?.ToString(),
                    GarmentHash = row[18]?.ToString() ?? string.Empty,
                    CreatedAt = ParseDate(row[19]) ?? DateTime.UtcNow,
                    UpdatedAt = ParseDate(row[20])
                });
            }

            // Legacy schema with feature_json.
            // 0 garment_id
            // 1 owner_user_id/user_id
            // 2 type
            // 3 color
            // 4 fit
            // 5 material
            // 6 sleeve
            // 7 length
            // 8 brand
            // 9 image_url
            // 10 feature_json
            // 11 garment_hash
            // 12 created_at
            // 13 updated_at
            if (row.Length >= 14)
            {
                return Task.FromResult(new Garment
                {
                    GarmentId = row[0]?.ToString() ?? string.Empty,
                    OwnerUserId = row[1]?.ToString() ?? string.Empty,
                    Type = row[2]?.ToString() ?? string.Empty,
                    Color = row[3]?.ToString(),
                    Fit = row[4]?.ToString(),
                    Material = row[5]?.ToString(),
                    Sleeve = row[6]?.ToString(),
                    Length = row[7]?.ToString(),
                    Brand = row[8]?.ToString(),
                    ImageUrl = row[9]?.ToString(),
                    FeatureJson = row[10]?.ToString(),
                    GarmentHash = row[11]?.ToString() ?? string.Empty,
                    CreatedAt = ParseDate(row[12]) ?? DateTime.UtcNow,
                    UpdatedAt = ParseDate(row[13])
                });
            }

            // Legacy schema without feature_json.
            if (row.Length >= 13)
            {
                return Task.FromResult(new Garment
                {
                    GarmentId = row[0]?.ToString() ?? string.Empty,
                    OwnerUserId = row[1]?.ToString() ?? string.Empty,
                    Type = row[2]?.ToString() ?? string.Empty,
                    Color = row[3]?.ToString(),
                    Fit = row[4]?.ToString(),
                    Material = row[5]?.ToString(),
                    Sleeve = row[6]?.ToString(),
                    Length = row[7]?.ToString(),
                    Brand = row[8]?.ToString(),
                    ImageUrl = row[9]?.ToString(),
                    GarmentHash = row[10]?.ToString() ?? string.Empty,
                    CreatedAt = ParseDate(row[11]) ?? DateTime.UtcNow,
                    UpdatedAt = ParseDate(row[12])
                });
            }

            // Very old fallback.
            return Task.FromResult(new Garment
            {
                GarmentId = row.Length > 0 ? row[0]?.ToString() ?? string.Empty : string.Empty,
                Type = row.Length > 1 ? row[1]?.ToString() ?? string.Empty : string.Empty,
                Color = row.Length > 2 ? row[2]?.ToString() : null,
                Brand = row.Length > 3 ? row[3]?.ToString() : null,
                CreatedAt = DateTime.UtcNow
            });
        }

        // הסבר: פונקציית בדיקה. מחזירה תשובה לוגית/ניסיון פריסה כדי להחליט על המשך הזרימה.
        private static bool IsUnknownColumn(MySqlException ex) => ex.Number == 1054;

        // הסבר: פונקציית פירסור. ממירה טקסט/JSON/פרמטרים למבנה נתונים שנוח לעבוד איתו.
        private static DateTime? ParseDate(object? value)
        {
            if (value == null) return null;
            return DateTime.TryParse(value.ToString(), out var dt) ? dt : null;
        }

        // הסבר: פונקציית בדיקה. מחזירה תשובה לוגית/ניסיון פריסה כדי להחליט על המשך הזרימה.
        private static int? TryParseNullableInt(object? value)
        {
            if (value == null) return null;
            var text = value.ToString();
            return int.TryParse(text, out var parsed) ? parsed : null;
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<List<Garment>> GetByUserAsync(string userId)
        {
            LastError = null;

            try
            {
                var sql = $"SELECT * FROM {GetTableName()} WHERE owner_user_id = @uid ORDER BY created_at DESC";
                return await SelectAllAsync(sql, new Dictionary<string, object> { ["uid"] = userId });
            }
            catch (MySqlException ex) when (IsUnknownColumn(ex))
            {
                try
                {
                    var sql = $"SELECT * FROM {GetTableName()} WHERE user_id = @uid ORDER BY created_at DESC";
                    return await SelectAllAsync(sql, new Dictionary<string, object> { ["uid"] = userId });
                }
                catch (MySqlException ex2)
                {
                    LastError = $"MySQL {ex2.Number}: {ex2.Message}";
                    return new List<Garment>();
                }
                catch (Exception ex2)
                {
                    LastError = ex2.Message;
                    return new List<Garment>();
                }
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

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public Task<List<Garment>> GetByUserIdAsync(string userId) => GetByUserAsync(userId);

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<Garment?> GetByIdAsync(string garmentId)
        {
            LastError = null;

            try
            {
                var sql = $"SELECT * FROM {GetTableName()} WHERE garment_id = @id";
                var list = await SelectAllAsync(sql, new Dictionary<string, object> { ["id"] = garmentId });
                return list.FirstOrDefault();
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

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        public async Task<int> CreateAsync(Garment g)
        {
            LastError = null;

            var strategies = new[]
            {
                new { UseOwnerId = true, IncludeFeatureJson = true, IncludeExtended = true },
                new { UseOwnerId = true, IncludeFeatureJson = false, IncludeExtended = true },
                new { UseOwnerId = true, IncludeFeatureJson = true, IncludeExtended = false },
                new { UseOwnerId = true, IncludeFeatureJson = false, IncludeExtended = false },
                new { UseOwnerId = false, IncludeFeatureJson = true, IncludeExtended = true },
                new { UseOwnerId = false, IncludeFeatureJson = false, IncludeExtended = true },
                new { UseOwnerId = false, IncludeFeatureJson = true, IncludeExtended = false },
                new { UseOwnerId = false, IncludeFeatureJson = false, IncludeExtended = false }
            };

            foreach (var strategy in strategies)
            {
                try
                {
                    var fields = BuildFieldDictionary(g, strategy.UseOwnerId, strategy.IncludeFeatureJson, strategy.IncludeExtended);
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

        // הסבר: פונקציית בנייה. מרכיבה אובייקט/בקשה/פלט מתוך נתוני קלט לפני השלב הבא בזרימה.
        private static Dictionary<string, object> BuildFieldDictionary(
            Garment g,
            bool useOwnerId,
            bool includeFeatureJson,
            bool includeExtended)
        {
            var values = new Dictionary<string, object>
            {
                ["garment_id"] = g.GarmentId,
                ["type"] = g.Type,
                ["color"] = (object?)g.Color ?? DBNull.Value,
                ["fit"] = (object?)g.Fit ?? DBNull.Value,
                ["material"] = (object?)g.Material ?? DBNull.Value,
                ["sleeve"] = (object?)g.Sleeve ?? DBNull.Value,
                ["length"] = (object?)g.Length ?? DBNull.Value,
                ["brand"] = (object?)g.Brand ?? DBNull.Value,
                ["image_url"] = (object?)g.ImageUrl ?? DBNull.Value,
                ["garment_hash"] = g.GarmentHash,
                ["created_at"] = g.CreatedAt == default ? DateTime.UtcNow : g.CreatedAt
            };

            var key = useOwnerId ? "owner_user_id" : "user_id";
            values[key] = g.OwnerUserId;

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

        // הסבר: פונקציית מחיקה. מסירה נתון קיים ומחזירה תוצאה כדי לאשר שהפעולה הושלמה.
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

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public async Task<int> CountByUserRoleAsync(string role)
        {
            LastError = null;
            var normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();

            try
            {
                return await CountByUserRoleInternalAsync(normalizedRole, "owner_user_id");
            }
            catch (MySqlException ex) when (IsUnknownColumn(ex))
            {
                try
                {
                    return await CountByUserRoleInternalAsync(normalizedRole, "user_id");
                }
                catch (Exception ex2)
                {
                    LastError = ex2.Message;
                    return 0;
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return 0;
            }
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<Dictionary<DateTime, int>> GetDailyCreatedCountsByUserRoleAsync(int days, string role)
        {
            LastError = null;
            var normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();
            var safeDays = Math.Clamp(days, 1, 365);
            var startUtc = DateTime.UtcNow.Date.AddDays(-(safeDays - 1));
            var endUtcExclusive = DateTime.UtcNow.Date.AddDays(1);

            try
            {
                return await GetDailyCreatedCountsByUserRoleInternalAsync(normalizedRole, startUtc, endUtcExclusive, "owner_user_id");
            }
            catch (MySqlException ex) when (IsUnknownColumn(ex))
            {
                try
                {
                    return await GetDailyCreatedCountsByUserRoleInternalAsync(normalizedRole, startUtc, endUtcExclusive, "user_id");
                }
                catch (Exception ex2)
                {
                    LastError = ex2.Message;
                    return new Dictionary<DateTime, int>();
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return new Dictionary<DateTime, int>();
            }
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public async Task<Dictionary<string, int>> CountByTypeAsync(string userId)
        {
            LastError = null;
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var sql = $@"SELECT type, COUNT(*) AS cnt
                             FROM {GetTableName()}
                             WHERE owner_user_id = @uid
                             GROUP BY type";
                var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object> { ["uid"] = userId });
                foreach (var row in rows)
                {
                    var type = NormalizeType(row[0]?.ToString());
                    if (int.TryParse(row[1]?.ToString(), out var c))
                    {
                        if (result.ContainsKey(type)) result[type] += c;
                        else result[type] = c;
                    }
                }
                LogCounts(userId, result);
                return result;
            }
            catch (MySqlException ex) when (IsUnknownColumn(ex))
            {
                try
                {
                    var sql = $@"SELECT type, COUNT(*) AS cnt
                                 FROM {GetTableName()}
                                 WHERE user_id = @uid
                                 GROUP BY type";
                    var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object> { ["uid"] = userId });
                    foreach (var row in rows)
                    {
                        var type = NormalizeType(row[0]?.ToString());
                        if (int.TryParse(row[1]?.ToString(), out var c))
                        {
                            if (result.ContainsKey(type)) result[type] += c;
                            else result[type] = c;
                        }
                    }
                    LogCounts(userId, result);
                    return result;
                }
                catch (Exception ex2)
                {
                    LastError = ex2.Message;
                    return result;
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return result;
            }
        }

        // הסבר: פונקציית נרמול. מנקה ומאחד פורמט נתונים כדי למנוע חוסר עקביות בהמשך הזרימה.
        private static string NormalizeType(string? raw)
        {
            var t = (raw ?? string.Empty).Trim().ToLowerInvariant();
            if (t.StartsWith("shirt")) return "shirt";
            if (t.StartsWith("pant") || t.StartsWith("trouser")) return "pants";
            if (t.StartsWith("jean")) return "pants";
            if (t.StartsWith("shoe")) return "shoes";
            if (t == "shoes") return "shoes";
            return t;
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private void LogCounts(string userId, Dictionary<string, int> counts)
        {
            try
            {
                var summary = string.Join(",", counts.Select(kv => $"{kv.Key}:{kv.Value}"));
                System.Diagnostics.Debug.WriteLine($"[CountByType] user={userId} counts={summary}");
            }
            catch
            {
                // ignored
            }
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
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

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
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

        // הסבר: פונקציית עדכון. משנה נתון קיים ושומרת את השינוי בצורה בטוחה.
        public async Task<int> UpdateFeaturesAsync(string garmentId, string featureJson)
        {
            LastError = null;
            var fields = new Dictionary<string, object>
            {
                ["feature_json"] = featureJson,
                ["updated_at"] = DateTime.UtcNow
            };

            var parameters = new Dictionary<string, object>
            {
                ["garment_id"] = garmentId
            };

            try
            {
                return await UpdateAsync(fields, parameters);
            }
            catch (MySqlException ex) when (IsUnknownColumn(ex))
            {
                LastError = $"feature_json column missing: {ex.Message}";
                return 0;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return 0;
            }
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<List<Garment>> GetByUserAndTypeAsync(string userId, string type, IEnumerable<string>? excludeIds = null)
        {
            LastError = null;
            var exList = excludeIds?.ToList() ?? new List<string>();

            try
            {
                var sql = $@"SELECT * FROM {GetTableName()}
                             WHERE owner_user_id = @uid AND type = @t";
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
            catch (MySqlException ex) when (IsUnknownColumn(ex))
            {
                try
                {
                    var sql = $@"SELECT * FROM {GetTableName()}
                                 WHERE user_id = @uid AND type = @t";
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
                catch (Exception ex2)
                {
                    LastError = ex2.Message;
                    return new List<Garment>();
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return new List<Garment>();
            }
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<List<Garment>> GetFilteredByUserAsync(string userId, GarmentFilterRequest? filter, int take = 600)
        {
            LastError = null;
            filter ??= new GarmentFilterRequest();
            filter.Normalize();

            try
            {
                return await GetFilteredByUserColumnAsync("owner_user_id", userId, filter, take);
            }
            catch (MySqlException ex) when (IsUnknownColumn(ex))
            {
                return await GetFilteredByUserColumnAsync("user_id", userId, filter, take);
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return new List<Garment>();
            }
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<GarmentFilterOptions> GetFilterOptionsAsync(string userId, GarmentFilterRequest? filter)
        {
            LastError = null;
            filter ??= new GarmentFilterRequest();
            filter.Normalize();

            try
            {
                return await GetFilterOptionsByUserColumnAsync("owner_user_id", userId, filter);
            }
            catch (MySqlException ex) when (IsUnknownColumn(ex))
            {
                return await GetFilterOptionsByUserColumnAsync("user_id", userId, filter);
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return new GarmentFilterOptions();
            }
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        private async Task<List<Garment>> GetFilteredByUserColumnAsync(
            string userColumn,
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
            var sql = $@"SELECT * FROM {GetTableName()}
                         {where}
                         ORDER BY created_at DESC
                         LIMIT @take";
            return await SelectAllAsync(sql, parameters);
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        private async Task<GarmentFilterOptions> GetFilterOptionsByUserColumnAsync(
            string userColumn,
            string userId,
            GarmentFilterRequest filter)
        {
            var options = new GarmentFilterOptions();

            options.Categories = await QueryDistinctCategoryOptionsAsync(userColumn, userId, filter);
            options.Subcategories = await QueryDistinctColumnOptionsAsync(userColumn, userId, filter, "style_category", "subcategories");
            options.Seasons = await QueryDistinctColumnOptionsAsync(userColumn, userId, filter, "season", "seasons");
            options.Materials = await QueryDistinctColumnOptionsAsync(userColumn, userId, filter, "material", "materials");
            options.Brands = await QueryDistinctColumnOptionsAsync(userColumn, userId, filter, "brand", "brands");
            options.Fits = await QueryDistinctColumnOptionsAsync(userColumn, userId, filter, "fit", "fits");
            options.Patterns = await QueryDistinctColumnOptionsAsync(userColumn, userId, filter, "pattern", "patterns");
            options.Colors = await QueryDistinctColorOptionsAsync(userColumn, userId, filter);
            options.Tags = await QueryDistinctTagOptionsAsync(userColumn, userId, filter);

            return options;
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
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
            var sql = $@"SELECT DISTINCT LOWER(TRIM({column})) AS value
                         FROM {GetTableName()}
                         {where}
                           AND {column} IS NOT NULL
                           AND TRIM({column}) <> ''
                         ORDER BY value";
            var rows = await StingListSelectAllAsync(sql, parameters);
            return rows
                .Select(r => r.Length > 0 ? r[0]?.ToString() ?? string.Empty : string.Empty)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
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
            var sql = $@"SELECT DISTINCT LOWER(TRIM(type)) AS value
                         FROM {GetTableName()}
                         {where}
                           AND type IS NOT NULL
                           AND TRIM(type) <> ''";
            var rows = await StingListSelectAllAsync(sql, parameters);

            var mapped = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows)
            {
                var raw = row.Length > 0 ? row[0]?.ToString() : null;
                var normalized = NormalizeType(raw);
                if (!string.IsNullOrWhiteSpace(normalized))
                    mapped.Add(normalized);
            }

            return mapped
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
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
                ) c
                WHERE value IS NOT NULL AND TRIM(value) <> ''
                ORDER BY value";

            var rows = await StingListSelectAllAsync(sql, parameters);
            return rows
                .Select(r => r.Length > 0 ? r[0]?.ToString() ?? string.Empty : string.Empty)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
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

        // הסבר: פונקציית פירסור. ממירה טקסט/JSON/פרמטרים למבנה נתונים שנוח לעבוד איתו.
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
                // fallback to CSV-like parsing
            }

            foreach (var item in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!string.IsNullOrWhiteSpace(item))
                    result.Add(item.Trim().Trim('"', '\'').ToLowerInvariant());
            }

            return result;
        }

        // הסבר: פונקציית בנייה. מרכיבה אובייקט/בקשה/פלט מתוך נתוני קלט לפני השלב הבא בזרימה.
        private static string BuildGarmentWhereClause(
            GarmentFilterRequest filter,
            Dictionary<string, object> parameters,
            string? excludedFilterKey = null,
            string userColumn = "owner_user_id")
        {
            var clauses = new List<string>
            {
                $"{userColumn} = @uid"
            };

            if (!string.Equals(excludedFilterKey, "categories", StringComparison.OrdinalIgnoreCase))
                AddCategoryClause(clauses, parameters, filter.Categories);

            if (!string.Equals(excludedFilterKey, "subcategories", StringComparison.OrdinalIgnoreCase))
                AddInClause(clauses, parameters, "style_category", filter.Subcategories, "sub");

            if (!string.Equals(excludedFilterKey, "colors", StringComparison.OrdinalIgnoreCase) && filter.Colors.Count > 0)
                AddColorClause(clauses, parameters, filter.Colors);

            if (!string.Equals(excludedFilterKey, "seasons", StringComparison.OrdinalIgnoreCase))
                AddInClause(clauses, parameters, "season", filter.Seasons, "sea");

            if (!string.Equals(excludedFilterKey, "materials", StringComparison.OrdinalIgnoreCase))
                AddInClause(clauses, parameters, "material", filter.Materials, "mat");

            if (!string.Equals(excludedFilterKey, "brands", StringComparison.OrdinalIgnoreCase))
                AddInClause(clauses, parameters, "brand", filter.Brands, "br");

            if (!string.Equals(excludedFilterKey, "fits", StringComparison.OrdinalIgnoreCase))
                AddInClause(clauses, parameters, "fit", filter.Fits, "fit");

            if (!string.Equals(excludedFilterKey, "patterns", StringComparison.OrdinalIgnoreCase))
                AddInClause(clauses, parameters, "pattern", filter.Patterns, "pat");

            if (!string.Equals(excludedFilterKey, "tags", StringComparison.OrdinalIgnoreCase) && filter.Tags.Count > 0)
                AddTagLikeClause(clauses, parameters, filter.Tags);

            return $"WHERE {string.Join(" AND ", clauses)}";
        }

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        private static void AddInClause(
            List<string> clauses,
            Dictionary<string, object> parameters,
            string column,
            List<string> values,
            string paramPrefix)
        {
            if (values.Count == 0)
                return;

            var placeholders = new List<string>();
            for (var i = 0; i < values.Count; i++)
            {
                var name = $"{paramPrefix}{i}";
                placeholders.Add($"@{name}");
                parameters[name] = values[i];
            }

            clauses.Add($"LOWER(TRIM({column})) IN ({string.Join(",", placeholders)})");
        }

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        private static void AddCategoryClause(
            List<string> clauses,
            Dictionary<string, object> parameters,
            List<string> categories)
        {
            if (categories.Count == 0)
                return;

            var normalized = categories
                .Select(NormalizeType)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalized.Count == 0)
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

            clauses.Add($"({string.Join(" OR ", predicates)})");
        }

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        private static void AddColorClause(
            List<string> clauses,
            Dictionary<string, object> parameters,
            List<string> values)
        {
            var placeholders = new List<string>();
            for (var i = 0; i < values.Count; i++)
            {
                var name = $"col{i}";
                placeholders.Add($"@{name}");
                parameters[name] = values[i];
            }

            var inList = string.Join(",", placeholders);
            clauses.Add($"(LOWER(TRIM(color)) IN ({inList}) OR LOWER(TRIM(color_secondary)) IN ({inList}))");
        }

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
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
