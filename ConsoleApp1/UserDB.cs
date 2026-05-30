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
    public class UserDB : BaseDB<User>
    {
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        protected override string GetPrimaryKeyName() => "user_id";

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        protected override string GetTableName() => "eitan_project12.users";

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        protected override async Task<User> CreateModelAsync(object[] row)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var u = new User
            {
                UserId = row[0]?.ToString() ?? "",
                Email = row[1]?.ToString() ?? "",
                FullName = row[2]?.ToString() ?? "",
                Role = row[3]?.ToString() ?? "customer",
                PasswordHash = row[4]?.ToString() ?? "",
                CreatedAt = DateTime.TryParse(row[5]?.ToString(), out var created)
                    ? created
                    : DateTime.UtcNow
            };

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            return await Task.FromResult(u);
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<List<User>> GetAllAsync()
        {
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            return await SelectAllAsync();
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<User?> GetByIdAsync(string userId)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sql = "SELECT * FROM eitan_project12.users WHERE user_id = @id";
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var list = await SelectAllAsync(sql,
                new Dictionary<string, object> { { "id", userId } });
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return list.FirstOrDefault();
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<List<User>> GetByEmailAsync(string email)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sql = "SELECT * FROM eitan_project12.users WHERE email = @em";
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            return await SelectAllAsync(sql,
                new Dictionary<string, object> { { "em", email } });
        }


        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<User?> GetSingleByEmailAsync(string email)
        {
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var list = await GetByEmailAsync(email);
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return list.FirstOrDefault();
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<int> CreateAsync(User u)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var fields = new Dictionary<string, object>
            {
                ["user_id"] = u.UserId,
                ["email"] = u.Email,
                ["full_name"] = u.FullName,
                ["role"] = u.Role,
                ["password_hash"] = u.PasswordHash,
                ["created_at"] = u.CreatedAt
            };

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            return await InsertAsync(fields);
        }



        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<int> UpdateUserAsync(User u)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var fields = new Dictionary<string, object>
            {
                ["email"] = u.Email,
                ["full_name"] = u.FullName,
                ["role"] = u.Role
            };

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var parameters = new Dictionary<string, object>
            {
                ["user_id"] = u.UserId
            };

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            return await UpdateAsync(fields, parameters);
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public Task<int> UpdatePasswordHashAsync(string userId, string passwordHash)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var fields = new Dictionary<string, object>
            {
                ["password_hash"] = passwordHash
            };

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var parameters = new Dictionary<string, object>
            {
                ["user_id"] = userId
            };

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return UpdateAsync(fields, parameters);
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public Task<int> DeleteUserAsync(string userId)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var parameters = new Dictionary<string, object>
            {
                ["user_id"] = userId
            };

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return DeleteAsync(parameters);
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<int> CountAllAsync()
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sql = "SELECT COUNT(*) FROM eitan_project12.users";
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>());
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return ParseCount(rows);
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<int> CountByRoleAsync(string role)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sql = @"SELECT COUNT(*)
                        FROM eitan_project12.users
                        WHERE LOWER(TRIM(COALESCE(role,''))) = @role";
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>
            {
                ["role"] = normalizedRole
            });
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return ParseCount(rows);
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<Dictionary<DateTime, int>> GetDailyCreatedCountsAsync(int days, string? role = null)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var safeDays = Math.Clamp(days, 1, 365);
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var startUtc = DateTime.UtcNow.Date.AddDays(-(safeDays - 1));
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var endUtcExclusive = DateTime.UtcNow.Date.AddDays(1);
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var hasRole = !string.IsNullOrWhiteSpace(role);
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sql = hasRole
                ? @"SELECT DATE(created_at) AS day_key, COUNT(*) AS cnt
                    FROM eitan_project12.users
                    WHERE created_at >= @startUtc
                      AND created_at < @endUtcExclusive
                      AND LOWER(TRIM(COALESCE(role,''))) = @role
                    GROUP BY DATE(created_at)
                    ORDER BY day_key"
                : @"SELECT DATE(created_at) AS day_key, COUNT(*) AS cnt
                    FROM eitan_project12.users
                    WHERE created_at >= @startUtc
                      AND created_at < @endUtcExclusive
                    GROUP BY DATE(created_at)
                    ORDER BY day_key";

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var parameters = new Dictionary<string, object>
            {
                ["startUtc"] = startUtc,
                ["endUtcExclusive"] = endUtcExclusive
            };
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (hasRole)
                parameters["role"] = normalizedRole;

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var rows = await StingListSelectAllAsync(sql, parameters);
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




    }
}
