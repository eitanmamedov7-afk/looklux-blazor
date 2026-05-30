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
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Models;
using MySql.Data.MySqlClient;

// הגדרת מרחו שמות שממקם את הקובץ בטבקת הפרויקט המטאימה.
namespace DBL
{
    // הגדרת מבנה מרכזי שמרכז נתונים או פעוליות עובר החלק הזה בפרויקט.
    public class GarmentDB : BaseDB<Garment>
    {
        // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
        public string? LastError { get; private set; }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        protected override string GetPrimaryKeyName() => "garment_id";
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        protected override string GetTableName() => "eitan_project12.garments";

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        protected override Task<Garment> CreateModelAsync(object[] row)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (row.Length == 20)
            {
                // החזרת התוצאה אל הקוד שקרא לפעולה.
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

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (row.Length >= 21)
            {
                // החזרת התוצאה אל הקוד שקרא לפעולה.
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

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (row.Length >= 14)
            {
                // החזרת התוצאה אל הקוד שקרא לפעולה.
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

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (row.Length >= 13)
            {
                // החזרת התוצאה אל הקוד שקרא לפעולה.
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

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return Task.FromResult(new Garment
            {
                GarmentId = row.Length > 0 ? row[0]?.ToString() ?? string.Empty : string.Empty,
                Type = row.Length > 1 ? row[1]?.ToString() ?? string.Empty : string.Empty,
                Color = row.Length > 2 ? row[2]?.ToString() : null,
                Brand = row.Length > 3 ? row[3]?.ToString() : null,
                CreatedAt = DateTime.UtcNow
            });
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static bool IsUnknownColumn(MySqlException ex) => ex.Number == 1054;

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static DateTime? ParseDate(object? value)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (value == null) return null;
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return DateTime.TryParse(value.ToString(), out var dt) ? dt : null;
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static int? TryParseNullableInt(object? value)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (value == null) return null;
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var text = value.ToString();
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return int.TryParse(text, out var parsed) ? parsed : null;
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<List<Garment>> GetByUserAsync(string userId)
        {
            LastError = null;

            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var sql = $"SELECT * FROM {GetTableName()} WHERE owner_user_id = @uid ORDER BY created_at DESC";
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                return await SelectAllAsync(sql, new Dictionary<string, object> { ["uid"] = userId });
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (MySqlException ex) when (IsUnknownColumn(ex))
            {
                // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
                try
                {
                    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                    var sql = $"SELECT * FROM {GetTableName()} WHERE user_id = @uid ORDER BY created_at DESC";
                    // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                    return await SelectAllAsync(sql, new Dictionary<string, object> { ["uid"] = userId });
                }
                // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
                catch (MySqlException ex2)
                {
                    LastError = $"MySQL {ex2.Number}: {ex2.Message}";
                    // החזרת התוצאה אל הקוד שקרא לפעולה.
                    return new List<Garment>();
                }
                // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
                catch (Exception ex2)
                {
                    LastError = ex2.Message;
                    // החזרת התוצאה אל הקוד שקרא לפעולה.
                    return new List<Garment>();
                }
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (MySqlException ex)
            {
                LastError = $"MySQL {ex.Number}: {ex.Message}";
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return new List<Garment>();
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (Exception ex)
            {
                LastError = ex.Message;
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return new List<Garment>();
            }
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public Task<List<Garment>> GetByUserIdAsync(string userId) => GetByUserAsync(userId);

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<Garment?> GetByIdAsync(string garmentId)
        {
            LastError = null;

            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var sql = $"SELECT * FROM {GetTableName()} WHERE garment_id = @id";
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                var list = await SelectAllAsync(sql, new Dictionary<string, object> { ["id"] = garmentId });
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return list.FirstOrDefault();
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (MySqlException ex)
            {
                LastError = $"MySQL {ex.Number}: {ex.Message}";
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return null;
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (Exception ex)
            {
                LastError = ex.Message;
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return null;
            }
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<int> CreateAsync(Garment g)
        {
            LastError = null;

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
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

            // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
            foreach (var strategy in strategies)
            {
                // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
                try
                {
                    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                    var fields = BuildFieldDictionary(g, strategy.UseOwnerId, strategy.IncludeFeatureJson, strategy.IncludeExtended);
                    // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                    var inserted = await InsertAsync(fields);
                    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                    if (inserted > 0)
                        // החזרת התוצאה אל הקוד שקרא לפעולה.
                        return inserted;
                }
                // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
                catch (MySqlException ex) when (IsUnknownColumn(ex))
                {
                    LastError = $"MySQL {ex.Number}: {ex.Message}";
                    continue;
                }
                // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
                catch (MySqlException ex)
                {
                    LastError = $"MySQL {ex.Number}: {ex.Message}";
                    // החזרת התוצאה אל הקוד שקרא לפעולה.
                    return 0;
                }
                // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    // החזרת התוצאה אל הקוד שקרא לפעולה.
                    return 0;
                }
            }

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return 0;
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static Dictionary<string, object> BuildFieldDictionary(
            Garment g,
            bool useOwnerId,
            bool includeFeatureJson,
            bool includeExtended)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
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

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var key = useOwnerId ? "owner_user_id" : "user_id";
            values[key] = g.OwnerUserId;

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
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

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (includeFeatureJson)
                values["feature_json"] = (object?)g.FeatureJson ?? DBNull.Value;

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return values;
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<int> DeleteGarmentAsync(string garmentId)
        {
            LastError = null;

            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var parameters = new Dictionary<string, object>
                {
                    ["garment_id"] = garmentId
                };

                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                return await DeleteAsync(parameters);
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (MySqlException ex)
            {
                LastError = $"MySQL {ex.Number}: {ex.Message}";
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return 0;
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (Exception ex)
            {
                LastError = ex.Message;
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return 0;
            }
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<int> CountByUserRoleAsync(string role)
        {
            LastError = null;
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();

            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                return await CountByUserRoleInternalAsync(normalizedRole, "owner_user_id");
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (MySqlException ex) when (IsUnknownColumn(ex))
            {
                // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
                try
                {
                    // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                    return await CountByUserRoleInternalAsync(normalizedRole, "user_id");
                }
                // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
                catch (Exception ex2)
                {
                    LastError = ex2.Message;
                    // החזרת התוצאה אל הקוד שקרא לפעולה.
                    return 0;
                }
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (Exception ex)
            {
                LastError = ex.Message;
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return 0;
            }
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<Dictionary<DateTime, int>> GetDailyCreatedCountsByUserRoleAsync(int days, string role)
        {
            LastError = null;
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var safeDays = Math.Clamp(days, 1, 365);
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var startUtc = DateTime.UtcNow.Date.AddDays(-(safeDays - 1));
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var endUtcExclusive = DateTime.UtcNow.Date.AddDays(1);

            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                return await GetDailyCreatedCountsByUserRoleInternalAsync(normalizedRole, startUtc, endUtcExclusive, "owner_user_id");
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (MySqlException ex) when (IsUnknownColumn(ex))
            {
                // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
                try
                {
                    // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                    return await GetDailyCreatedCountsByUserRoleInternalAsync(normalizedRole, startUtc, endUtcExclusive, "user_id");
                }
                // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
                catch (Exception ex2)
                {
                    LastError = ex2.Message;
                    // החזרת התוצאה אל הקוד שקרא לפעולה.
                    return new Dictionary<DateTime, int>();
                }
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (Exception ex)
            {
                LastError = ex.Message;
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return new Dictionary<DateTime, int>();
            }
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<Dictionary<string, int>> CountByTypeAsync(string userId)
        {
            LastError = null;
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var sql = $@"SELECT type, COUNT(*) AS cnt
                             FROM {GetTableName()}
                             WHERE owner_user_id = @uid
                             GROUP BY type";
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object> { ["uid"] = userId });
                // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
                foreach (var row in rows)
                {
                    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                    var type = NormalizeType(row[0]?.ToString());
                    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                    if (int.TryParse(row[1]?.ToString(), out var c))
                    {
                        // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                        if (result.ContainsKey(type)) result[type] += c;
                        // מסלול חלופי שפועל כאשר התנאי הקודם לא התקיים.
                        else result[type] = c;
                    }
                }
                LogCounts(userId, result);
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return result;
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (MySqlException ex) when (IsUnknownColumn(ex))
            {
                // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
                try
                {
                    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                    var sql = $@"SELECT type, COUNT(*) AS cnt
                                 FROM {GetTableName()}
                                 WHERE user_id = @uid
                                 GROUP BY type";
                    // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                    var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object> { ["uid"] = userId });
                    // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
                    foreach (var row in rows)
                    {
                        // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                        var type = NormalizeType(row[0]?.ToString());
                        // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                        if (int.TryParse(row[1]?.ToString(), out var c))
                        {
                            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                            if (result.ContainsKey(type)) result[type] += c;
                            // מסלול חלופי שפועל כאשר התנאי הקודם לא התקיים.
                            else result[type] = c;
                        }
                    }
                    LogCounts(userId, result);
                    // החזרת התוצאה אל הקוד שקרא לפעולה.
                    return result;
                }
                // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
                catch (Exception ex2)
                {
                    LastError = ex2.Message;
                    // החזרת התוצאה אל הקוד שקרא לפעולה.
                    return result;
                }
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (Exception ex)
            {
                LastError = ex.Message;
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return result;
            }
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static string NormalizeType(string? raw)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var t = (raw ?? string.Empty).Trim().ToLowerInvariant();
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (t.StartsWith("shirt")) return "shirt";
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (t.StartsWith("pant") || t.StartsWith("trouser")) return "pants";
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (t.StartsWith("jean")) return "pants";
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (t.StartsWith("shoe")) return "shoes";
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (t == "shoes") return "shoes";
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return t;
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private void LogCounts(string userId, Dictionary<string, int> counts)
        {
            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
                var summary = string.Join(",", counts.Select(kv => $"{kv.Key}:{kv.Value}"));
                System.Diagnostics.Debug.WriteLine($"[CountByType] user={userId} counts={summary}");
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch
            {
            }
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        private async Task<int> CountByUserRoleInternalAsync(string normalizedRole, string ownerColumn)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sql = $@"SELECT COUNT(*)
                         FROM {GetTableName()} g
                         INNER JOIN eitan_project12.users u ON u.user_id = g.{ownerColumn}
                         WHERE LOWER(TRIM(COALESCE(u.role,''))) = @role";
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>
            {
                ["role"] = normalizedRole
            });
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return ParseCount(rows);
        }

        private async Task<Dictionary<DateTime, int>> GetDailyCreatedCountsByUserRoleInternalAsync(
            string normalizedRole,
            DateTime startUtc,
            DateTime endUtcExclusive,
            string ownerColumn)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sql = $@"SELECT DATE(g.created_at) AS day_key, COUNT(*) AS cnt
                         FROM {GetTableName()} g
                         INNER JOIN eitan_project12.users u ON u.user_id = g.{ownerColumn}
                         WHERE g.created_at >= @startUtc
                           AND g.created_at < @endUtcExclusive
                           AND LOWER(TRIM(COALESCE(u.role,''))) = @role
                         GROUP BY DATE(g.created_at)
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

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<int> UpdateFeaturesAsync(string garmentId, string featureJson)
        {
            LastError = null;
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var fields = new Dictionary<string, object>
            {
                ["feature_json"] = featureJson,
                ["updated_at"] = DateTime.UtcNow
            };

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var parameters = new Dictionary<string, object>
            {
                ["garment_id"] = garmentId
            };

            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                return await UpdateAsync(fields, parameters);
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (MySqlException ex) when (IsUnknownColumn(ex))
            {
                LastError = $"feature_json column missing: {ex.Message}";
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return 0;
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (Exception ex)
            {
                LastError = ex.Message;
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return 0;
            }
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<List<Garment>> GetByUserAndTypeAsync(string userId, string type, IEnumerable<string>? excludeIds = null)
        {
            LastError = null;
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var exList = excludeIds?.ToList() ?? new List<string>();

            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var sql = $@"SELECT * FROM {GetTableName()}
                             WHERE owner_user_id = @uid AND type = @t";
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (exList.Count > 0)
                {
                    // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
                    var placeholders = string.Join(",", exList.Select((_, i) => $"@ex{i}"));
                    sql += $" AND garment_id NOT IN ({placeholders})";
                }
                sql += " ORDER BY created_at DESC";

                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var parameters = new Dictionary<string, object>
                {
                    ["uid"] = userId,
                    ["t"] = type
                };
                // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
                for (int i = 0; i < exList.Count; i++)
                    parameters[$"ex{i}"] = exList[i];

                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                return await SelectAllAsync(sql, parameters);
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (MySqlException ex) when (IsUnknownColumn(ex))
            {
                // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
                try
                {
                    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                    var sql = $@"SELECT * FROM {GetTableName()}
                                 WHERE user_id = @uid AND type = @t";
                    // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                    if (exList.Count > 0)
                    {
                        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
                        var placeholders = string.Join(",", exList.Select((_, i) => $"@ex{i}"));
                        sql += $" AND garment_id NOT IN ({placeholders})";
                    }
                    sql += " ORDER BY created_at DESC";

                    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                    var parameters = new Dictionary<string, object>
                    {
                        ["uid"] = userId,
                        ["t"] = type
                    };
                    // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
                    for (int i = 0; i < exList.Count; i++)
                        parameters[$"ex{i}"] = exList[i];

                    // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                    return await SelectAllAsync(sql, parameters);
                }
                // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
                catch (Exception ex2)
                {
                    LastError = ex2.Message;
                    // החזרת התוצאה אל הקוד שקרא לפעולה.
                    return new List<Garment>();
                }
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (Exception ex)
            {
                LastError = ex.Message;
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return new List<Garment>();
            }
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<List<Garment>> GetFilteredByUserAsync(string userId, GarmentFilterRequest? filter, int take = 600)
        {
            LastError = null;
            filter ??= new GarmentFilterRequest();
            filter.Normalize();

            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                return await GetFilteredByUserColumnAsync("owner_user_id", userId, filter, take);
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (MySqlException ex) when (IsUnknownColumn(ex))
            {
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                return await GetFilteredByUserColumnAsync("user_id", userId, filter, take);
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (Exception ex)
            {
                LastError = ex.Message;
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return new List<Garment>();
            }
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<GarmentFilterOptions> GetFilterOptionsAsync(string userId, GarmentFilterRequest? filter)
        {
            LastError = null;
            filter ??= new GarmentFilterRequest();
            filter.Normalize();

            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                return await GetFilterOptionsByUserColumnAsync("owner_user_id", userId, filter);
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (MySqlException ex) when (IsUnknownColumn(ex))
            {
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                return await GetFilterOptionsByUserColumnAsync("user_id", userId, filter);
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (Exception ex)
            {
                LastError = ex.Message;
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return new GarmentFilterOptions();
            }
        }

        private async Task<List<Garment>> GetFilteredByUserColumnAsync(
            string userColumn,
            string userId,
            GarmentFilterRequest filter,
            int take)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var parameters = new Dictionary<string, object>
            {
                ["uid"] = userId,
                ["take"] = Math.Max(1, take)
            };

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var where = BuildGarmentWhereClause(filter, parameters, null, userColumn);
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sql = $@"SELECT * FROM {GetTableName()}
                         {where}
                         ORDER BY created_at DESC
                         LIMIT @take";
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            return await SelectAllAsync(sql, parameters);
        }

        private async Task<GarmentFilterOptions> GetFilterOptionsByUserColumnAsync(
            string userColumn,
            string userId,
            GarmentFilterRequest filter)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var options = new GarmentFilterOptions();

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            options.Categories = await QueryDistinctCategoryOptionsAsync(userColumn, userId, filter);
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            options.Subcategories = await QueryDistinctColumnOptionsAsync(userColumn, userId, filter, "style_category", "subcategories");
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            options.Seasons = await QueryDistinctColumnOptionsAsync(userColumn, userId, filter, "season", "seasons");
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            options.Materials = await QueryDistinctColumnOptionsAsync(userColumn, userId, filter, "material", "materials");
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            options.Brands = await QueryDistinctColumnOptionsAsync(userColumn, userId, filter, "brand", "brands");
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            options.Fits = await QueryDistinctColumnOptionsAsync(userColumn, userId, filter, "fit", "fits");
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            options.Patterns = await QueryDistinctColumnOptionsAsync(userColumn, userId, filter, "pattern", "patterns");
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            options.Colors = await QueryDistinctColorOptionsAsync(userColumn, userId, filter);
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            options.Tags = await QueryDistinctTagOptionsAsync(userColumn, userId, filter);

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return options;
        }

        private async Task<List<string>> QueryDistinctColumnOptionsAsync(
            string userColumn,
            string userId,
            GarmentFilterRequest filter,
            string column,
            string excludedFilterKey)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var parameters = new Dictionary<string, object>
            {
                ["uid"] = userId
            };

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var where = BuildGarmentWhereClause(filter, parameters, excludedFilterKey, userColumn);
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sql = $@"SELECT DISTINCT LOWER(TRIM({column})) AS value
                         FROM {GetTableName()}
                         {where}
                           AND {column} IS NOT NULL
                           AND TRIM({column}) <> ''
                         ORDER BY value";
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var rows = await StingListSelectAllAsync(sql, parameters);
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return rows
                .Select(r => r.Length > 0 ? r[0]?.ToString() ?? string.Empty : string.Empty)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task<List<string>> QueryDistinctCategoryOptionsAsync(
            string userColumn,
            string userId,
            GarmentFilterRequest filter)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var parameters = new Dictionary<string, object>
            {
                ["uid"] = userId
            };

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var where = BuildGarmentWhereClause(filter, parameters, "categories", userColumn);
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sql = $@"SELECT DISTINCT LOWER(TRIM(type)) AS value
                         FROM {GetTableName()}
                         {where}
                           AND type IS NOT NULL
                           AND TRIM(type) <> ''";
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var rows = await StingListSelectAllAsync(sql, parameters);

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var mapped = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
            foreach (var row in rows)
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var raw = row.Length > 0 ? row[0]?.ToString() : null;
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var normalized = NormalizeType(raw);
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (!string.IsNullOrWhiteSpace(normalized))
                    mapped.Add(normalized);
            }

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return mapped
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task<List<string>> QueryDistinctColorOptionsAsync(
            string userColumn,
            string userId,
            GarmentFilterRequest filter)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var parameters = new Dictionary<string, object>
            {
                ["uid"] = userId
            };

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var where = BuildGarmentWhereClause(filter, parameters, "colors", userColumn);
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
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

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var rows = await StingListSelectAllAsync(sql, parameters);
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return rows
                .Select(r => r.Length > 0 ? r[0]?.ToString() ?? string.Empty : string.Empty)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task<List<string>> QueryDistinctTagOptionsAsync(
            string userColumn,
            string userId,
            GarmentFilterRequest filter)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var parameters = new Dictionary<string, object>
            {
                ["uid"] = userId
            };

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var where = BuildGarmentWhereClause(filter, parameters, "tags", userColumn);
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sql = $@"SELECT style_tags
                         FROM {GetTableName()}
                         {where}
                           AND style_tags IS NOT NULL
                           AND TRIM(style_tags) <> ''";

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var rows = await StingListSelectAllAsync(sql, parameters);
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
            foreach (var row in rows)
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var raw = row.Length > 0 ? row[0]?.ToString() : null;
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
                foreach (var tag in ParseStyleTags(raw))
                    tags.Add(tag);
            }

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return tags.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static List<string> ParseStyleTags(string raw)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var result = new List<string>();

            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var doc = JsonDocument.Parse(raw);
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
                    foreach (var item in doc.RootElement.EnumerateArray())
                    {
                        // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                        var value = item.GetString();
                        // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                        if (!string.IsNullOrWhiteSpace(value))
                            result.Add(value.Trim().ToLowerInvariant());
                    }
                    // החזרת התוצאה אל הקוד שקרא לפעולה.
                    return result;
                }
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch
            {
            }

            // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
            foreach (var item in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (!string.IsNullOrWhiteSpace(item))
                    result.Add(item.Trim().Trim('"', '\'').ToLowerInvariant());
            }

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return result;
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static string BuildGarmentWhereClause(
            GarmentFilterRequest filter,
            Dictionary<string, object> parameters,
            string? excludedFilterKey = null,
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            string userColumn = "owner_user_id")
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var clauses = new List<string>
            {
                $"{userColumn} = @uid"
            };

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (!string.Equals(excludedFilterKey, "categories", StringComparison.OrdinalIgnoreCase))
                AddCategoryClause(clauses, parameters, filter.Categories);

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (!string.Equals(excludedFilterKey, "subcategories", StringComparison.OrdinalIgnoreCase))
                AddInClause(clauses, parameters, "style_category", filter.Subcategories, "sub");

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (!string.Equals(excludedFilterKey, "colors", StringComparison.OrdinalIgnoreCase) && filter.Colors.Count > 0)
                AddColorClause(clauses, parameters, filter.Colors);

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (!string.Equals(excludedFilterKey, "seasons", StringComparison.OrdinalIgnoreCase))
                AddInClause(clauses, parameters, "season", filter.Seasons, "sea");

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (!string.Equals(excludedFilterKey, "materials", StringComparison.OrdinalIgnoreCase))
                AddInClause(clauses, parameters, "material", filter.Materials, "mat");

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (!string.Equals(excludedFilterKey, "brands", StringComparison.OrdinalIgnoreCase))
                AddInClause(clauses, parameters, "brand", filter.Brands, "br");

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (!string.Equals(excludedFilterKey, "fits", StringComparison.OrdinalIgnoreCase))
                AddInClause(clauses, parameters, "fit", filter.Fits, "fit");

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (!string.Equals(excludedFilterKey, "patterns", StringComparison.OrdinalIgnoreCase))
                AddInClause(clauses, parameters, "pattern", filter.Patterns, "pat");

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (!string.Equals(excludedFilterKey, "tags", StringComparison.OrdinalIgnoreCase) && filter.Tags.Count > 0)
                AddTagLikeClause(clauses, parameters, filter.Tags);

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return $"WHERE {string.Join(" AND ", clauses)}";
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static void AddInClause(
            List<string> clauses,
            Dictionary<string, object> parameters,
            string column,
            List<string> values,
            string paramPrefix)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (values.Count == 0)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return;

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var placeholders = new List<string>();
            // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
            for (var i = 0; i < values.Count; i++)
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var name = $"{paramPrefix}{i}";
                placeholders.Add($"@{name}");
                parameters[name] = values[i];
            }

            clauses.Add($"LOWER(TRIM({column})) IN ({string.Join(",", placeholders)})");
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static void AddCategoryClause(
            List<string> clauses,
            Dictionary<string, object> parameters,
            List<string> categories)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (categories.Count == 0)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return;

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var normalized = categories
                .Select(NormalizeType)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (normalized.Count == 0)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return;

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var predicates = new List<string>();
            // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
            for (var i = 0; i < normalized.Count; i++)
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var value = normalized[i];
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var exact = $"catExact{i}";
                parameters[exact] = value;

                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (string.Equals(value, "shirt", StringComparison.OrdinalIgnoreCase))
                {
                    predicates.Add($"(LOWER(TRIM(type)) = @{exact} OR LOWER(TRIM(type)) LIKE @catLike{i}a OR LOWER(TRIM(type)) LIKE @catLike{i}b)");
                    parameters[$"catLike{i}a"] = "%tee%";
                    parameters[$"catLike{i}b"] = "%top%";
                }
                // מסלול חלופי שפועל כאשר התנאי הקודם לא התקיים.
                else if (string.Equals(value, "pants", StringComparison.OrdinalIgnoreCase))
                {
                    predicates.Add($"(LOWER(TRIM(type)) = @{exact} OR LOWER(TRIM(type)) LIKE @catLike{i}a OR LOWER(TRIM(type)) LIKE @catLike{i}b)");
                    parameters[$"catLike{i}a"] = "%trouser%";
                    parameters[$"catLike{i}b"] = "%jean%";
                }
                // מסלול חלופי שפועל כאשר התנאי הקודם לא התקיים.
                else if (string.Equals(value, "shoes", StringComparison.OrdinalIgnoreCase))
                {
                    predicates.Add($"(LOWER(TRIM(type)) = @{exact} OR LOWER(TRIM(type)) LIKE @catLike{i}a OR LOWER(TRIM(type)) LIKE @catLike{i}b)");
                    parameters[$"catLike{i}a"] = "%sneaker%";
                    parameters[$"catLike{i}b"] = "%boot%";
                }
                // מסלול חלופי שפועל כאשר התנאי הקודם לא התקיים.
                else
                {
                    predicates.Add($"LOWER(TRIM(type)) = @{exact}");
                }
            }

            clauses.Add($"({string.Join(" OR ", predicates)})");
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static void AddColorClause(
            List<string> clauses,
            Dictionary<string, object> parameters,
            List<string> values)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var placeholders = new List<string>();
            // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
            for (var i = 0; i < values.Count; i++)
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var name = $"col{i}";
                placeholders.Add($"@{name}");
                parameters[name] = values[i];
            }

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var inList = string.Join(",", placeholders);
            clauses.Add($"(LOWER(TRIM(color)) IN ({inList}) OR LOWER(TRIM(color_secondary)) IN ({inList}))");
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static void AddTagLikeClause(
            List<string> clauses,
            Dictionary<string, object> parameters,
            List<string> tags)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var builder = new StringBuilder();
            builder.Append('(');

            // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
            for (var i = 0; i < tags.Count; i++)
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var name = $"tag{i}";
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
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
