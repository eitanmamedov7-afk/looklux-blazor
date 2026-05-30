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
using MySql.Data.MySqlClient;

// הגדרת מרחו שמות שממקם את הקובץ בטבקת הפרויקט המטאימה.
namespace DBL
{
    // הגדרת מבנה מרכזי שמרכז נתונים או פעוליות עובר החלק הזה בפרויקט.
    public class OutfitGarmentDB : BaseDB<OutfitGarment>
    {
        private HashSet<string>? _columnCache;

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        protected override string GetPrimaryKeyName() => "outfit_garment_id";
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        protected override string GetTableName() => "eitan_project12.outfit_garments";

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        protected override Task<OutfitGarment> CreateModelAsync(object[] row)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (row.Length >= 6)
            {
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return Task.FromResult(new OutfitGarment
                {
                    OutfitGarmentId = row[0]?.ToString() ?? string.Empty,
                    OutfitId = row[1]?.ToString() ?? string.Empty,
                    GarmentId = row[2]?.ToString() ?? string.Empty,
                    GarmentType = NormalizeRole(row[3]?.ToString()),
                    IsSeed = Convert.ToInt32(row[4] ?? 0) != 0,
                    CreatedAt = DateTime.TryParse(row[5]?.ToString(), out var created) ? created : DateTime.UtcNow
                });
            }

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return Task.FromResult(new OutfitGarment
            {
                OutfitGarmentId = string.Empty,
                OutfitId = row.Length > 0 ? row[0]?.ToString() ?? string.Empty : string.Empty,
                GarmentId = row.Length > 1 ? row[1]?.ToString() ?? string.Empty : string.Empty,
                GarmentType = NormalizeRole(row.Length > 2 ? row[2]?.ToString() : null),
                IsSeed = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<int> CreateAsync(OutfitGarment item)
        {
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var columns = await GetColumnSetAsync();
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var hasRole = columns.Contains("role");
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var hasNewColumns = columns.Contains("outfit_garment_id") && columns.Contains("garment_type");

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (hasRole && !hasNewColumns)
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var legacyFields = new Dictionary<string, object>
                {
                    ["outfit_id"] = item.OutfitId,
                    ["garment_id"] = item.GarmentId,
                    ["role"] = NormalizeRole(item.GarmentType)
                };

                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                return await InsertAsync(legacyFields);
            }

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var fields = new Dictionary<string, object>
            {
                ["outfit_garment_id"] = string.IsNullOrWhiteSpace(item.OutfitGarmentId)
                    ? Guid.NewGuid().ToString()
                    : item.OutfitGarmentId,
                ["outfit_id"] = item.OutfitId,
                ["garment_id"] = item.GarmentId,
                ["garment_type"] = NormalizeRole(item.GarmentType),
                ["is_seed"] = item.IsSeed ? 1 : 0,
                ["created_at"] = item.CreatedAt == default ? DateTime.UtcNow : item.CreatedAt
            };

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            return await InsertAsync(fields);
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<bool> CreateForOutfitAsync(string outfitId, IEnumerable<OutfitGarment> rows)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var allOk = true;
            // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
            foreach (var row in rows)
            {
                row.OutfitId = outfitId;
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (row.CreatedAt == default) row.CreatedAt = DateTime.UtcNow;

                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                var inserted = await CreateAsync(row);
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (inserted <= 0)
                    allOk = false;
            }

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return allOk;
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<List<OutfitGarment>> GetByOutfitIdsAsync(IEnumerable<string> outfitIds)
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
                return new List<OutfitGarment>();

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var columns = await GetColumnSetAsync();
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var hasRole = columns.Contains("role");
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var hasNewColumns = columns.Contains("outfit_garment_id") && columns.Contains("garment_type");

            // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
            var placeholders = string.Join(",", ids.Select((_, i) => $"@id{i}"));
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var parameters = new Dictionary<string, object>();
            // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
            for (int i = 0; i < ids.Count; i++)
                parameters[$"id{i}"] = ids[i];

            string sql;
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (hasRole && !hasNewColumns)
            {
                sql = $@"SELECT outfit_id, garment_id, role
                         FROM {GetTableName()}
                         WHERE outfit_id IN ({placeholders})";
            }
            // מסלול חלופי שפועל כאשר התנאי הקודם לא התקיים.
            else
            {
                sql = $@"SELECT outfit_garment_id, outfit_id, garment_id, garment_type, is_seed, created_at
                         FROM {GetTableName()}
                         WHERE outfit_id IN ({placeholders})";
            }

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            return await SelectAllAsync(sql, parameters);
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<int> DeleteByOutfitIdAsync(string outfitId)
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
        private async Task<HashSet<string>> GetColumnSetAsync()
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (_columnCache != null)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return _columnCache;

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sql = @"SELECT LOWER(column_name)
                        FROM information_schema.columns
                        WHERE table_schema = 'eitan_project12'
                          AND table_name = 'outfit_garments'";

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>());
            _columnCache = rows
                .Select(r => r.Length > 0 ? r[0]?.ToString() ?? string.Empty : string.Empty)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim().ToLowerInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return _columnCache;
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static string NormalizeRole(string? raw)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var value = (raw ?? string.Empty).Trim().ToLowerInvariant();
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
    }
}
