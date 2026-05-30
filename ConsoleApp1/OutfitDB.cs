// מה הקובץ עושה: הקובץ מרכז חלק מהמערכת ומשתתף בהפעלת הפרויקט.
// למה הקובץ נדרש: הוא נדרש כדי שהחלק הזה בפרויקט יפעל בצורה ברורה ומסודרת.
// לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר למסכים, לשירותים, למודלים ולשכבת הדיבי לפי השימוש שלו.
// איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים לקבצים שמזמנים את הקוד הזה או לקבצים שהוא מזמן.

// מה הקובץ עושה: הקובץ מטפל בגישה למסד הנתונים ובתרגום נתונים לשכבט הקוד.
// הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
// לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר למודלם, לשירותים, לדפי הניהול, לדף הארון ולשירות ההתאמות.
// איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים במודלם שמוחזרים מכאן ובדפים או בשירותים שקוראים לפעוליות הדיבי.



// ייבוא ספריות שמספקות מחלקות, ממשקים ופעולות שהקובץ צריך כדי לעבוד.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;

// הגדרת מרחו שמות שממקם את הקובץ בטבקת הפרויקט המטאימה.
namespace DBL
{
    // הגדרת מבנה מרכזי שמרכז נתונים או פעוליות עובר החלק הזה בפרויקט.
    public class OutfitDB : BaseDB<Outfit>
    {
        private HashSet<string>? _outfitColumns;
        private HashSet<string>? _outfitGarmentColumns;
        private HashSet<string>? _garmentColumns;

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        protected override string GetPrimaryKeyName() => "outfit_id";
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        protected override string GetTableName() => "eitan_project12.outfits";

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        protected override Task<Outfit> CreateModelAsync(object[] row)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (row.Length >= 15)
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var requestedOrSeed = row[13]?.ToString();
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var firstRequested = FirstRequestedId(requestedOrSeed);
                // החזרת התוצאה אל הקוד שקרא לפעולה.
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

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (row.Length >= 11)
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var requested = row[9]?.ToString();
                // החזרת התוצאה אל הקוד שקרא לפעולה.
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

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (row.Length >= 10)
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var seedGarmentId = row[8]?.ToString();
                // החזרת התוצאה אל הקוד שקרא לפעולה.
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

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return Task.FromResult(new Outfit
            {
                OutfitId = row.Length > 0 ? row[0]?.ToString() ?? string.Empty : string.Empty,
                CreatedAt = DateTime.UtcNow
            });
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<List<Outfit>> GetAllAsync()
        {
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            return await SelectAllAsync($"SELECT * FROM {GetTableName()}");
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<List<Outfit>> GetByUserAsync(string userId, int take = 50)
        {
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            return await GetFilteredByUserAsync(userId, new OutfitFilterRequest(), take);
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<Outfit?> GetByIdAsync(string outfitId)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sql = $"SELECT * FROM {GetTableName()} WHERE outfit_id = @id LIMIT 1";
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var rows = await SelectAllAsync(sql, new Dictionary<string, object> { ["id"] = outfitId });
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return rows.FirstOrDefault();
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<int> DeleteOutfitAsync(string outfitId)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (string.IsNullOrWhiteSpace(outfitId))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return 0;

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            return await DeleteAsync(new Dictionary<string, object>
            {
                ["outfit_id"] = outfitId.Trim()
            });
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<int> CountByUserRoleAsync(string role)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var outfitColumns = await GetOutfitColumnsAsync();

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("user_id"))
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var sql = $@"SELECT COUNT(*)
                             FROM {GetTableName()} o
                             INNER JOIN eitan_project12.users u ON u.user_id = o.user_id
                             WHERE LOWER(TRIM(COALESCE(u.role,''))) = @role";
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>
                {
                    ["role"] = normalizedRole
                });
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return ParseCount(rows);
            }

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var garmentColumns = await GetGarmentColumnsAsync();
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var ownerColumn = garmentColumns.Contains("owner_user_id") ? "owner_user_id" : "user_id";

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var legacySql = $@"SELECT COUNT(DISTINCT o.outfit_id)
                               FROM {GetTableName()} o
                               INNER JOIN eitan_project12.outfit_garments og ON og.outfit_id = o.outfit_id
                               INNER JOIN eitan_project12.garments g ON g.garment_id = og.garment_id
                               INNER JOIN eitan_project12.users u ON u.user_id = g.{ownerColumn}
                               WHERE LOWER(TRIM(COALESCE(u.role,''))) = @role";
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var legacyRows = await StingListSelectAllAsync(legacySql, new Dictionary<string, object>
            {
                ["role"] = normalizedRole
            });
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return ParseCount(legacyRows);
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<Dictionary<DateTime, int>> GetDailyCreatedCountsByUserRoleAsync(int days, string role)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var safeDays = Math.Clamp(days, 1, 365);
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var startUtc = DateTime.UtcNow.Date.AddDays(-(safeDays - 1));
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var endUtcExclusive = DateTime.UtcNow.Date.AddDays(1);
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var outfitColumns = await GetOutfitColumnsAsync();
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var createdExpr = outfitColumns.Contains("created_at") ? "o.created_at" : "CURRENT_TIMESTAMP";

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("user_id"))
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var sql = $@"SELECT DATE({createdExpr}) AS day_key, COUNT(*) AS cnt
                             FROM {GetTableName()} o
                             INNER JOIN eitan_project12.users u ON u.user_id = o.user_id
                             WHERE {createdExpr} >= @startUtc
                               AND {createdExpr} < @endUtcExclusive
                               AND LOWER(TRIM(COALESCE(u.role,''))) = @role
                             GROUP BY DATE({createdExpr})
                             ORDER BY day_key";
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>
                {
                    ["startUtc"] = startUtc,
                    ["endUtcExclusive"] = endUtcExclusive,
                    ["role"] = normalizedRole
                });
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return ParseDailyCounts(rows);
            }

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var garmentColumns = await GetGarmentColumnsAsync();
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var ownerColumn = garmentColumns.Contains("owner_user_id") ? "owner_user_id" : "user_id";

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
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
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var legacyRows = await StingListSelectAllAsync(legacySql, new Dictionary<string, object>
            {
                ["startUtc"] = startUtc,
                ["endUtcExclusive"] = endUtcExclusive,
                ["role"] = normalizedRole
            });
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return ParseDailyCounts(legacyRows);
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<int> CreateAsync(Outfit o)
        {
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var outfitColumns = await GetOutfitColumnsAsync();
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Count == 0)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return 0;

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var seedGarmentId = FirstNonEmpty(o.SeedGarmentId, FirstRequestedId(o.RequestedGarmentIds));
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var requestedGarmentIds = !string.IsNullOrWhiteSpace(o.RequestedGarmentIds)
                ? o.RequestedGarmentIds
                : seedGarmentId;

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var fields = new Dictionary<string, object>
            {
                ["outfit_id"] = string.IsNullOrWhiteSpace(o.OutfitId) ? Guid.NewGuid().ToString() : o.OutfitId
            };

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("user_id"))
                fields["user_id"] = o.UserId;

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("shirt_garment_id"))
                fields["shirt_garment_id"] = (object?)o.ShirtGarmentId ?? DBNull.Value;
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("pants_garment_id"))
                fields["pants_garment_id"] = (object?)o.PantsGarmentId ?? DBNull.Value;
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("shoes_garment_id"))
                fields["shoes_garment_id"] = (object?)o.ShoesGarmentId ?? DBNull.Value;

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("score"))
                fields["score"] = o.Score;
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("rank"))
                fields["rank"] = o.Rank;

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("style_label"))
                fields["style_label"] = (object?)o.StyleLabel ?? DBNull.Value;
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("outfit_style"))
                fields["outfit_style"] = (object?)o.StyleLabel ?? DBNull.Value;

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("explanation"))
                fields["explanation"] = (object?)o.Explanation ?? DBNull.Value;

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("recommended_places"))
                fields["recommended_places"] = (object?)o.RecommendedPlaces ?? DBNull.Value;
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("recommended_where"))
                fields["recommended_where"] = (object?)o.RecommendedPlaces ?? DBNull.Value;
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("where_to_wear"))
                fields["where_to_wear"] = (object?)o.RecommendedPlaces ?? DBNull.Value;

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("seed_type"))
                fields["seed_type"] = (object?)o.SeedType ?? DBNull.Value;

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("seed_garment_id"))
                fields["seed_garment_id"] = (object?)seedGarmentId ?? DBNull.Value;

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("label_is_compatible"))
                fields["label_is_compatible"] = o.LabelIsCompatible ? 1 : 0;
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("label_source"))
                fields["label_source"] = string.IsNullOrWhiteSpace(o.LabelSource) ? "auto" : o.LabelSource;
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("requested_garment_ids"))
                fields["requested_garment_ids"] = (object?)requestedGarmentIds ?? DBNull.Value;
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("created_at"))
                fields["created_at"] = o.CreatedAt == default ? DateTime.UtcNow : o.CreatedAt;

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            return await InsertAsync(fields);
        }

        public async Task<bool> ExistsDuplicateAsync(
            string userId,
            string shirtGarmentId,
            string pantsGarmentId,
            string shoesGarmentId,
            string? seedType,
            string? seedGarmentId)
        {
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var outfitColumns = await GetOutfitColumnsAsync();
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var normalizedSeedType = (seedType ?? string.Empty).Trim().ToLowerInvariant();
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var normalizedSeedGarmentId = (seedGarmentId ?? string.Empty).Trim();

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfitColumns.Contains("user_id") &&
                outfitColumns.Contains("shirt_garment_id") &&
                outfitColumns.Contains("pants_garment_id") &&
                outfitColumns.Contains("shoes_garment_id"))
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var seedColumn = outfitColumns.Contains("seed_garment_id")
                    ? "seed_garment_id"
                    : outfitColumns.Contains("requested_garment_ids")
                        ? "requested_garment_ids"
                        : string.Empty;

                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var sql = $@"SELECT outfit_id
                             FROM {GetTableName()}
                             WHERE user_id = @uid
                               AND shirt_garment_id = @shirt
                               AND pants_garment_id = @pants
                               AND shoes_garment_id = @shoes";

                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var parameters = new Dictionary<string, object>
                {
                    ["uid"] = userId,
                    ["shirt"] = shirtGarmentId,
                    ["pants"] = pantsGarmentId,
                    ["shoes"] = shoesGarmentId
                };

                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (!string.IsNullOrWhiteSpace(normalizedSeedType) && outfitColumns.Contains("seed_type"))
                {
                    sql += " AND LOWER(TRIM(COALESCE(seed_type,''))) = @seedType";
                    parameters["seedType"] = normalizedSeedType;
                }

                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (!string.IsNullOrWhiteSpace(normalizedSeedGarmentId) && !string.IsNullOrWhiteSpace(seedColumn))
                {
                    sql += $" AND {seedColumn} = @seedGarmentId";
                    parameters["seedGarmentId"] = normalizedSeedGarmentId;
                }

                sql += " LIMIT 1";
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                var rows = await StingListSelectAllAsync(sql, parameters);
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return rows.Count > 0;
            }

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var linkColumns = await GetOutfitGarmentColumnsAsync();
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var garmentColumns = await GetGarmentColumnsAsync();
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var ownerColumn = garmentColumns.Contains("owner_user_id") ? "owner_user_id" : "user_id";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var roleExpr = BuildRoleNormalizationExpression(linkColumns, "og");
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var seedExpr = ResolveOutfitExpression(outfitColumns, "o.seed_type", "NULL");
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var seedGarmentExpr = ResolveOutfitExpression(outfitColumns, "o.seed_garment_id", "o.requested_garment_ids");

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
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

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var legacyParams = new Dictionary<string, object>
            {
                ["uid"] = userId,
                ["shirt"] = shirtGarmentId,
                ["pants"] = pantsGarmentId,
                ["shoes"] = shoesGarmentId
            };

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (!string.IsNullOrWhiteSpace(normalizedSeedType) && !string.Equals(seedExpr, "NULL", StringComparison.Ordinal))
            {
                legacySql += $" AND LOWER(TRIM(COALESCE({seedExpr},''))) = @seedType";
                legacyParams["seedType"] = normalizedSeedType;
            }

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (!string.IsNullOrWhiteSpace(normalizedSeedGarmentId) && !string.Equals(seedGarmentExpr, "NULL", StringComparison.Ordinal))
            {
                legacySql += $" AND {seedGarmentExpr} = @seedGarmentId";
                legacyParams["seedGarmentId"] = normalizedSeedGarmentId;
            }

            legacySql += " LIMIT 1";
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var legacyRows = await StingListSelectAllAsync(legacySql, legacyParams);
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return legacyRows.Count > 0;
        }


        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<List<Outfit>> GetFilteredByUserAsync(string userId, OutfitFilterRequest? filter, int take = 200)
        {
            filter ??= new OutfitFilterRequest();
            filter.Normalize();

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var outfitColumns = await GetOutfitColumnsAsync();
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var rawOutfits = outfitColumns.Contains("user_id")
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                ? await QueryModernBaseAsync(userId, filter, Math.Max(400, take * 4))
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                : await QueryLegacyBaseAsync(userId, filter, Math.Max(400, take * 4));

            Dictionary<string, OutfitFacts>? factsByOutfit = null;
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var needsFacts =
                filter.GarmentTypes.Count > 0 ||
                filter.Occasions.Count > 0 ||
                filter.Seasons.Count > 0;

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (needsFacts)
            {
                // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
                factsByOutfit = await QueryOutfitFactsByOutfitAsync(userId, rawOutfits.Select(x => x.OutfitId));
            }

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var filtered = rawOutfits
                .Where(o => MatchesOutfitFilter(o, filter, factsByOutfit))
                .OrderByDescending(o => o.CreatedAt)
                .Take(Math.Max(1, take))
                .ToList();

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return filtered;
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<OutfitFilterOptions> GetFilterOptionsAsync(string userId, OutfitFilterRequest? filter)
        {
            filter ??= new OutfitFilterRequest();
            filter.Normalize();

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var outfits = await GetFilteredByUserAsync(userId, filter, 1200);
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
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

            // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
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

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return options;
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        private async Task<List<Outfit>> QueryModernBaseAsync(string userId, OutfitFilterRequest filter, int take)
        {
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var columns = await GetOutfitColumnsAsync();
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (!columns.Contains("user_id"))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return new List<Outfit>();
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var garmentColumns = await GetGarmentColumnsAsync();
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var ownerColumn = garmentColumns.Contains("owner_user_id") ? "owner_user_id" : "user_id";

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var scoreExpr = columns.Contains("score") ? "o.score" : "0";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var rankExpr = columns.Contains("rank") ? "o.`rank`" : "1";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var createdExpr = columns.Contains("created_at") ? "o.created_at" : "CURRENT_TIMESTAMP";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var styleExpr = ResolveOutfitExpression(columns, "o.style_label", "o.outfit_style");
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var placesExpr = ResolveOutfitExpression(
                columns,
                "o.recommended_places",
                ResolveOutfitExpression(columns, "o.recommended_where", "o.where_to_wear"));
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var seedExpr = columns.Contains("seed_type") ? "o.seed_type" : "NULL";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var explanationExpr = columns.Contains("explanation") ? "o.explanation" : "NULL";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var labelCompatibleExpr = columns.Contains("label_is_compatible") ? "o.label_is_compatible" : "0";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var labelSourceExpr = columns.Contains("label_source") ? "o.label_source" : "'auto'";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var requestedExpr = ResolveOutfitExpression(columns, "o.requested_garment_ids", "o.seed_garment_id");
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var shirtExpr = columns.Contains("shirt_garment_id") ? "o.shirt_garment_id" : "NULL";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var pantsExpr = columns.Contains("pants_garment_id") ? "o.pants_garment_id" : "NULL";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var shoesExpr = columns.Contains("shoes_garment_id") ? "o.shoes_garment_id" : "NULL";

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
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

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            return await SelectAllAsync(sql, new Dictionary<string, object>
            {
                ["uid"] = userId,
                ["minScore"] = filter.MinScore,
                ["maxScore"] = filter.MaxScore,
                ["take"] = Math.Max(1, take)
            });
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        private async Task<List<Outfit>> QueryLegacyBaseAsync(string userId, OutfitFilterRequest filter, int take)
        {
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var outfitColumns = await GetOutfitColumnsAsync();
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var linkColumns = await GetOutfitGarmentColumnsAsync();
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var garmentColumns = await GetGarmentColumnsAsync();
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var ownerColumn = garmentColumns.Contains("owner_user_id") ? "owner_user_id" : "user_id";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var roleExpr = BuildRoleNormalizationExpression(linkColumns, "og");

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var scoreExpr = outfitColumns.Contains("score") ? "o.score" : "0";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var createdExpr = outfitColumns.Contains("created_at") ? "o.created_at" : "CURRENT_TIMESTAMP";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var styleExpr = ResolveOutfitExpression(outfitColumns, "o.style_label", "o.outfit_style");
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var placesExpr = ResolveOutfitExpression(
                outfitColumns,
                "o.recommended_places",
                ResolveOutfitExpression(outfitColumns, "o.recommended_where", "o.where_to_wear"));
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var seedExpr = outfitColumns.Contains("seed_type") ? "o.seed_type" : "NULL";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var explanationExpr = outfitColumns.Contains("explanation") ? "o.explanation" : "NULL";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var labelCompatibleExpr = outfitColumns.Contains("label_is_compatible") ? "o.label_is_compatible" : "0";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var labelSourceExpr = outfitColumns.Contains("label_source") ? "o.label_source" : "'auto'";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var requestedExpr = ResolveOutfitExpression(outfitColumns, "o.requested_garment_ids", "o.seed_garment_id");

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
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

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            return await SelectAllAsync(sql, new Dictionary<string, object>
            {
                ["uid"] = userId,
                ["minScore"] = filter.MinScore,
                ["maxScore"] = filter.MaxScore,
                ["take"] = Math.Max(1, take)
            });
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        private async Task<Dictionary<string, OutfitFacts>> QueryOutfitFactsByOutfitAsync(string userId, IEnumerable<string> outfitIds)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var ids = outfitIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (ids.Count == 0)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return new Dictionary<string, OutfitFacts>(StringComparer.OrdinalIgnoreCase);

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var linkColumns = await GetOutfitGarmentColumnsAsync();
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var garmentColumns = await GetGarmentColumnsAsync();
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (linkColumns.Count == 0)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return new Dictionary<string, OutfitFacts>(StringComparer.OrdinalIgnoreCase);

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var roleExpr = BuildRoleNormalizationExpression(linkColumns, "og");
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var ownerColumn = garmentColumns.Contains("owner_user_id") ? "owner_user_id" : "user_id";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var hasOccasion = garmentColumns.Contains("occasion");
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var hasSeason = garmentColumns.Contains("season");

            // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
            var placeholders = string.Join(",", ids.Select((_, i) => $"@id{i}"));
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var parameters = new Dictionary<string, object> { ["uid"] = userId };
            // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
            for (var i = 0; i < ids.Count; i++)
                parameters[$"id{i}"] = ids[i];

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var occasionExpr = hasOccasion ? "LOWER(TRIM(COALESCE(g.occasion,'')))" : "''";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var seasonExpr = hasSeason ? "LOWER(TRIM(COALESCE(g.season,'')))" : "''";

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sql = $@"SELECT og.outfit_id,
                                {roleExpr} AS garment_type,
                                {occasionExpr} AS occasion,
                                {seasonExpr} AS season
                         FROM eitan_project12.outfit_garments og
                         INNER JOIN eitan_project12.garments g ON g.garment_id = og.garment_id
                         WHERE og.outfit_id IN ({placeholders})
                           AND g.{ownerColumn} = @uid";

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var rows = await StingListSelectAllAsync(sql, parameters);
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var result = new Dictionary<string, OutfitFacts>(StringComparer.OrdinalIgnoreCase);

            // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
            foreach (var row in rows)
            {
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (row.Length < 4)
                    continue;

                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var outfitId = row[0]?.ToString();
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (string.IsNullOrWhiteSpace(outfitId))
                    continue;

                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (!result.TryGetValue(outfitId, out var facts))
                {
                    facts = new OutfitFacts();
                    result[outfitId] = facts;
                }

                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var type = NormalizeTypeName(row[1]?.ToString());
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (!string.IsNullOrWhiteSpace(type))
                    facts.GarmentTypes.Add(type);

                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var occasion = (row[2]?.ToString() ?? string.Empty).Trim().ToLowerInvariant();
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (!string.IsNullOrWhiteSpace(occasion))
                    facts.Occasions.Add(occasion);

                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var season = (row[3]?.ToString() ?? string.Empty).Trim().ToLowerInvariant();
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (!string.IsNullOrWhiteSpace(season))
                    facts.Seasons.Add(season);
            }

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return result;
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static bool MatchesOutfitFilter(
            Outfit outfit,
            OutfitFilterRequest filter,
            Dictionary<string, OutfitFacts>? factsByOutfit)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (outfit.Score < filter.MinScore || outfit.Score > filter.MaxScore)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return false;

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (filter.SeedTypes.Count > 0)
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var value = (outfit.SeedType ?? string.Empty).Trim().ToLowerInvariant();
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (!filter.SeedTypes.Contains(value, StringComparer.OrdinalIgnoreCase))
                    // החזרת התוצאה אל הקוד שקרא לפעולה.
                    return false;
            }

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (filter.StyleLabels.Count > 0)
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var value = (outfit.StyleLabel ?? string.Empty).Trim().ToLowerInvariant();
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (!filter.StyleLabels.Contains(value, StringComparer.OrdinalIgnoreCase))
                    // החזרת התוצאה אל הקוד שקרא לפעולה.
                    return false;
            }

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (filter.RecommendedPlaces.Count > 0)
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var tokens = SplitRecommendationTokens(outfit.RecommendedPlaces);
                // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
                var match = tokens.Any(token => filter.RecommendedPlaces.Any(sel => token.Contains(sel, StringComparison.OrdinalIgnoreCase)));
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (!match)
                    // החזרת התוצאה אל הקוד שקרא לפעולה.
                    return false;
            }

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (factsByOutfit == null ||
                (!filter.GarmentTypes.Any() && !filter.Occasions.Any() && !filter.Seasons.Any()))
            {
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return true;
            }

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (!factsByOutfit.TryGetValue(outfit.OutfitId, out var facts))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return false;

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (filter.GarmentTypes.Count > 0)
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var selectedTypes = filter.GarmentTypes
                    .Select(NormalizeTypeName)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (!facts.GarmentTypes.Overlaps(selectedTypes))
                    // החזרת התוצאה אל הקוד שקרא לפעולה.
                    return false;
            }

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (filter.Occasions.Count > 0 && !facts.Occasions.Overlaps(filter.Occasions))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return false;

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (filter.Seasons.Count > 0 && !facts.Seasons.Overlaps(filter.Seasons))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return false;

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return true;
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        private async Task<HashSet<string>> GetOutfitColumnsAsync()
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (_outfitColumns != null)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return _outfitColumns;

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            _outfitColumns = await QueryColumnSetAsync("outfits");
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return _outfitColumns;
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        private async Task<HashSet<string>> GetOutfitGarmentColumnsAsync()
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (_outfitGarmentColumns != null)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return _outfitGarmentColumns;

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            _outfitGarmentColumns = await QueryColumnSetAsync("outfit_garments");
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return _outfitGarmentColumns;
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        private async Task<HashSet<string>> GetGarmentColumnsAsync()
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (_garmentColumns != null)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return _garmentColumns;

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            _garmentColumns = await QueryColumnSetAsync("garments");
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return _garmentColumns;
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        private async Task<HashSet<string>> QueryColumnSetAsync(string tableName)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sql = @"SELECT LOWER(column_name)
                        FROM information_schema.columns
                        WHERE table_schema = 'eitan_project12'
                          AND table_name = @tableName";

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>
            {
                ["tableName"] = tableName
            });

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return rows
                .Select(r => r.Length > 0 ? r[0]?.ToString() ?? string.Empty : string.Empty)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim().ToLowerInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static string ResolveOutfitExpression(HashSet<string> columns, string primaryExpression, string fallbackExpression)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var primaryColumn = ExtractColumnName(primaryExpression);
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (columns.Contains(primaryColumn))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return primaryExpression;

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var fallbackColumn = ExtractColumnName(fallbackExpression);
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (columns.Contains(fallbackColumn))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return fallbackExpression;

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return "NULL";
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static string ExtractColumnName(string expression)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var value = expression;
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var dot = value.LastIndexOf('.');
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (dot >= 0)
                value = value[(dot + 1)..];
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var space = value.IndexOf(' ');
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (space >= 0)
                value = value[..space];
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return value.Trim().ToLowerInvariant();
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static string BuildRoleNormalizationExpression(HashSet<string> linkColumns, string alias)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var roleSource = linkColumns.Contains("garment_type")
                ? $"LOWER(TRIM({alias}.garment_type))"
                : linkColumns.Contains("role")
                    ? $"LOWER(TRIM({alias}.role))"
                    : "''";

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return $@"(CASE
                        WHEN {roleSource} LIKE '%shirt%' OR {roleSource} LIKE '%tee%' OR {roleSource} LIKE '%top%' THEN 'shirt'
                        WHEN {roleSource} LIKE '%pant%' OR {roleSource} LIKE '%trouser%' OR {roleSource} LIKE '%jean%' OR {roleSource} LIKE '%bottom%' THEN 'pants'
                        WHEN {roleSource} LIKE '%shoe%' OR {roleSource} LIKE '%sneaker%' OR {roleSource} LIKE '%boot%' OR {roleSource} LIKE '%foot%' THEN 'shoes'
                        ELSE {roleSource}
                      END)";
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static IEnumerable<string> SplitRecommendationTokens(string? places)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (string.IsNullOrWhiteSpace(places))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return Enumerable.Empty<string>();

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return places
                .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => x.Trim().ToLowerInvariant())
                .Where(x => !string.IsNullOrWhiteSpace(x));
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static int ParseScore(object? value)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (value == null)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return 0;

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (value is int i)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return Math.Clamp(i, 0, 100);

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (value is long l)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return Math.Clamp((int)l, 0, 100);

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (value is decimal d)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return Math.Clamp((int)Math.Round(d), 0, 100);

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (value is double db)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return Math.Clamp((int)Math.Round(db), 0, 100);

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (int.TryParse(value.ToString(), out var parsedInt))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return Math.Clamp(parsedInt, 0, 100);

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (decimal.TryParse(value.ToString(), out var parsedDecimal))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return Math.Clamp((int)Math.Round(parsedDecimal), 0, 100);

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return 0;
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static int ParseCount(List<object[]> rows)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (rows.Count == 0 || rows[0].Length == 0)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return 0;

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var raw = rows[0][0]?.ToString();
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (long.TryParse(raw, out var asLong))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return (int)Math.Clamp(asLong, 0, int.MaxValue);

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (int.TryParse(raw, out var asInt))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return Math.Max(0, asInt);

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return 0;
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static Dictionary<DateTime, int> ParseDailyCounts(List<object[]> rows)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var result = new Dictionary<DateTime, int>();
            // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
            foreach (var row in rows)
            {
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (row.Length < 2)
                    continue;

                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (!DateTime.TryParse(row[0]?.ToString(), out var day))
                    continue;

                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var raw = row[1]?.ToString();
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var count = 0;
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (long.TryParse(raw, out var asLong))
                    count = (int)Math.Clamp(asLong, 0, int.MaxValue);
                // מסלול חלופי שפועל כאשר התנאי הקודם לא התקיים.
                else if (int.TryParse(raw, out var asInt))
                    count = Math.Max(0, asInt);

                result[day.Date] = count;
            }

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return result;
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static string NormalizeTypeName(string? type)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var value = (type ?? string.Empty).Trim().ToLowerInvariant();
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (string.IsNullOrWhiteSpace(value))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return string.Empty;
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (value.Contains("shirt") || value.Contains("tee") || value.Contains("top"))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return "shirt";
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (value.Contains("pant") || value.Contains("trouser") || value.Contains("jean") || value.Contains("bottom"))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return "pants";
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (value.Contains("shoe") || value.Contains("sneaker") || value.Contains("boot") || value.Contains("foot"))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return "shoes";
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return value;
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static string? FirstRequestedId(string? requestedGarmentIds)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (string.IsNullOrWhiteSpace(requestedGarmentIds))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return null;

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return requestedGarmentIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static string? FirstNonEmpty(params string?[] values)
        {
            // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
            foreach (var value in values)
            {
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (!string.IsNullOrWhiteSpace(value))
                    // החזרת התוצאה אל הקוד שקרא לפעולה.
                    return value.Trim();
            }

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return null;
        }

        private sealed class OutfitFacts
        {
            // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
            public HashSet<string> GarmentTypes { get; } = new(StringComparer.OrdinalIgnoreCase);
            // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
            public HashSet<string> Occasions { get; } = new(StringComparer.OrdinalIgnoreCase);
            // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
            public HashSet<string> Seasons { get; } = new(StringComparer.OrdinalIgnoreCase);
        }
    }
}
