// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;

namespace DBL
{
    public class UserDB : BaseDB<User>
    {
        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        protected override string GetPrimaryKeyName() => "user_id";

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        protected override string GetTableName() => "eitan_project12.users";

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        protected override async Task<User> CreateModelAsync(object[] row)
        {
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

            return await Task.FromResult(u);
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<List<User>> GetAllAsync()
        {
            return await SelectAllAsync();
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<User?> GetByIdAsync(string userId)
        {
            var sql = "SELECT * FROM eitan_project12.users WHERE user_id = @id";
            var list = await SelectAllAsync(sql,
                new Dictionary<string, object> { { "id", userId } });
            return list.FirstOrDefault();
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<List<User>> GetByEmailAsync(string email)
        {
            var sql = "SELECT * FROM eitan_project12.users WHERE email = @em";
            return await SelectAllAsync(sql,
                new Dictionary<string, object> { { "em", email } });
        }


        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<User?> GetSingleByEmailAsync(string email)
        {
            var list = await GetByEmailAsync(email);
            return list.FirstOrDefault();
        }

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        public async Task<int> CreateAsync(User u)
        {
            var fields = new Dictionary<string, object>
            {
                ["user_id"] = u.UserId,
                ["email"] = u.Email,
                ["full_name"] = u.FullName,
                ["role"] = u.Role,
                ["password_hash"] = u.PasswordHash,
                ["created_at"] = u.CreatedAt
            };

            return await InsertAsync(fields);
        }



        // הסבר: פונקציית עדכון. משנה נתון קיים ושומרת את השינוי בצורה בטוחה.
        public async Task<int> UpdateUserAsync(User u)
        {
            var fields = new Dictionary<string, object>
            {
                ["email"] = u.Email,
                ["full_name"] = u.FullName,
                ["role"] = u.Role
            };

            var parameters = new Dictionary<string, object>
            {
                ["user_id"] = u.UserId
            };

            return await UpdateAsync(fields, parameters);
        }

        // הסבר: פונקציית עדכון. משנה נתון קיים ושומרת את השינוי בצורה בטוחה.
        public Task<int> UpdatePasswordHashAsync(string userId, string passwordHash)
        {
            var fields = new Dictionary<string, object>
            {
                ["password_hash"] = passwordHash
            };

            var parameters = new Dictionary<string, object>
            {
                ["user_id"] = userId
            };

            return UpdateAsync(fields, parameters);
        }

        // הסבר: פונקציית מחיקה. מסירה נתון קיים ומחזירה תוצאה כדי לאשר שהפעולה הושלמה.
        public Task<int> DeleteUserAsync(string userId)
        {
            var parameters = new Dictionary<string, object>
            {
                ["user_id"] = userId
            };

            return DeleteAsync(parameters);
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public async Task<int> CountAllAsync()
        {
            var sql = "SELECT COUNT(*) FROM eitan_project12.users";
            var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>());
            return ParseCount(rows);
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public async Task<int> CountByRoleAsync(string role)
        {
            var normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();
            var sql = @"SELECT COUNT(*)
                        FROM eitan_project12.users
                        WHERE LOWER(TRIM(COALESCE(role,''))) = @role";
            var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>
            {
                ["role"] = normalizedRole
            });
            return ParseCount(rows);
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<Dictionary<DateTime, int>> GetDailyCreatedCountsAsync(int days, string? role = null)
        {
            var safeDays = Math.Clamp(days, 1, 365);
            var startUtc = DateTime.UtcNow.Date.AddDays(-(safeDays - 1));
            var endUtcExclusive = DateTime.UtcNow.Date.AddDays(1);
            var hasRole = !string.IsNullOrWhiteSpace(role);
            var normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();

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

            var parameters = new Dictionary<string, object>
            {
                ["startUtc"] = startUtc,
                ["endUtcExclusive"] = endUtcExclusive
            };
            if (hasRole)
                parameters["role"] = normalizedRole;

            var rows = await StingListSelectAllAsync(sql, parameters);
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




    }
}
