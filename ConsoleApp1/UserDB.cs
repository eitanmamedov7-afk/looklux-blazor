



using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;

namespace DBL
{
    // Data-access class for application users.
    // UserDB connects the web/auth/admin pages to the eitan_project12.users table.
    // It inherits common SELECT/INSERT/UPDATE/DELETE helpers from BaseDB<User>
    // and only defines user-specific table names, model mapping, queries, and statistics.
    public class UserDB : BaseDB<User>
    {
        // BaseDB uses this column name when it needs to find a row by primary key.
        protected override string GetPrimaryKeyName() => "user_id";

        // BaseDB builds generic SQL against this table.
        protected override string GetTableName() => "eitan_project12.users";

        // Converts one raw database row into the User model used by the rest of the project.
        // The column order must match SELECT * from eitan_project12.users:
        // user_id, email, full_name, role, password_hash, created_at.
        protected override async Task<User> CreateModelAsync(object[] row)
        {
            // Map each database column into the C# User object.
            // Empty/default values protect the UI from null database values.
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

        // Returns every user account.
        // Used mainly by admin screens that list/manage customers and admins.
        public async Task<List<User>> GetAllAsync()
        {
            return await SelectAllAsync();
        }

        // Loads one user by id.
        // Useful when pages receive only a user id and need the full profile/role data.
        public async Task<User?> GetByIdAsync(string userId)
        {
            // Parameterized SQL fetches the exact account id without string-concatenating input.
            var sql = "SELECT * FROM eitan_project12.users WHERE user_id = @id";
            // BaseDB runs the query and converts rows through CreateModelAsync.
            var list = await SelectAllAsync(sql,
                new Dictionary<string, object> { { "id", userId } });
            return list.FirstOrDefault();
        }

        // Finds users by email.
        // Auth and registration use this to locate an account and to prevent duplicate emails.
        public async Task<List<User>> GetByEmailAsync(string email)
        {
            // Email lookup is used during login/register, so it stays parameterized.
            var sql = "SELECT * FROM eitan_project12.users WHERE email = @em";
            return await SelectAllAsync(sql,
                new Dictionary<string, object> { { "em", email } });
        }


        // Convenience wrapper for flows that expect only one account per email.
        public async Task<User?> GetSingleByEmailAsync(string email)
        {
            var list = await GetByEmailAsync(email);
            return list.FirstOrDefault();
        }

        // Inserts a new user account.
        // Registration/admin-create code prepares the User object, including the already-hashed password.
        public async Task<int> CreateAsync(User u)
        {
            // Field names match the users table columns that will be inserted.
            var fields = new Dictionary<string, object>
            {
                ["user_id"] = u.UserId,
                ["email"] = u.Email,
                ["full_name"] = u.FullName,
                ["role"] = u.Role,
                ["password_hash"] = u.PasswordHash,
                ["created_at"] = u.CreatedAt
            };

            // BaseDB builds the INSERT statement and executes it.
            return await InsertAsync(fields);
        }



        // Updates account profile fields that admins can edit.
        // Password changes are kept separate so normal profile edits do not accidentally touch credentials.
        public async Task<int> UpdateUserAsync(User u)
        {
            // Only editable profile/admin fields are updated here.
            var fields = new Dictionary<string, object>
            {
                ["email"] = u.Email,
                ["full_name"] = u.FullName,
                ["role"] = u.Role
            };

            // WHERE user_id = value; this prevents updating more than one account.
            var parameters = new Dictionary<string, object>
            {
                ["user_id"] = u.UserId
            };

            // BaseDB builds UPDATE users SET ... WHERE user_id = ...
            return await UpdateAsync(fields, parameters);
        }

        // Replaces a user's password hash.
        // Used by password reset/auth flows after the new password has already been hashed.
        public Task<int> UpdatePasswordHashAsync(string userId, string passwordHash)
        {
            // Store only the hash, never the plain password.
            var fields = new Dictionary<string, object>
            {
                ["password_hash"] = passwordHash
            };

            // Target the one account whose password is being reset/changed.
            var parameters = new Dictionary<string, object>
            {
                ["user_id"] = userId
            };

            return UpdateAsync(fields, parameters);
        }

        // Deletes a user row by id.
        // Admin deletion code is responsible for deleting related garments/outfits before calling this.
        public Task<int> DeleteUserAsync(string userId)
        {
            // Delete is scoped to one user id.
            var parameters = new Dictionary<string, object>
            {
                ["user_id"] = userId
            };

            return DeleteAsync(parameters);
        }

        // Counts all users for admin dashboard/statistics.
        public async Task<int> CountAllAsync()
        {
            // Dashboard total: one scalar COUNT(*) row.
            var sql = "SELECT COUNT(*) FROM eitan_project12.users";
            var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>());
            return ParseCount(rows);
        }

        // Counts users in one role, for example customer or admin.
        // Role text is normalized in SQL and C# so casing/spaces do not affect the result.
        public async Task<int> CountByRoleAsync(string role)
        {
            // Normalize role once before comparing it to normalized database values.
            var normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();
            // COALESCE protects the comparison if a role column value is null.
            var sql = @"SELECT COUNT(*)
                        FROM eitan_project12.users
                        WHERE LOWER(TRIM(COALESCE(role,''))) = @role";
            // The role parameter is passed separately from the SQL text.
            var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>
            {
                ["role"] = normalizedRole
            });
            return ParseCount(rows);
        }

        // Builds a day-by-day user creation count for dashboard charts.
        // The optional role filter lets the dashboard chart all users or only a specific role.
        public async Task<Dictionary<DateTime, int>> GetDailyCreatedCountsAsync(int days, string? role = null)
        {
            // Limit chart range to a practical window so bad input cannot request huge reports.
            var safeDays = Math.Clamp(days, 1, 365);
            // Use UTC date boundaries to match how created_at values are stored.
            var startUtc = DateTime.UtcNow.Date.AddDays(-(safeDays - 1));
            var endUtcExclusive = DateTime.UtcNow.Date.AddDays(1);
            // Optional role filter changes the WHERE clause below.
            var hasRole = !string.IsNullOrWhiteSpace(role);
            var normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();

            // Choose the SQL shape based on whether the caller requested one role or all users.
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

            // Date range parameters are always needed for this chart query.
            var parameters = new Dictionary<string, object>
            {
                ["startUtc"] = startUtc,
                ["endUtcExclusive"] = endUtcExclusive
            };
            // Add role only when the SQL includes the @role placeholder.
            if (hasRole)
                parameters["role"] = normalizedRole;

            // Raw grouped rows are parsed into DateTime -> count below.
            var rows = await StingListSelectAllAsync(sql, parameters);
            return ParseDailyCounts(rows);
        }

        // Converts a COUNT(*) result row into a safe non-negative int.
        // MySQL count values may arrive as long/string depending on provider behavior.
        private static int ParseCount(List<object[]> rows)
        {
            // No rows means the count query failed or returned nothing.
            if (rows.Count == 0 || rows[0].Length == 0)
                return 0;

            // Convert the first column of the first row into an int count.
            var raw = rows[0][0]?.ToString();
            if (long.TryParse(raw, out var asLong))
                return (int)Math.Clamp(asLong, 0, int.MaxValue);

            if (int.TryParse(raw, out var asInt))
                return Math.Max(0, asInt);

            return 0;
        }

        // Converts grouped SQL rows into chart data keyed by date.
        // Invalid rows are ignored so one malformed value does not break the dashboard.
        private static Dictionary<DateTime, int> ParseDailyCounts(List<object[]> rows)
        {
            // Result is keyed by day so dashboard code can quickly find a count for each date.
            var result = new Dictionary<DateTime, int>();
            foreach (var row in rows)
            {
                // Expected shape is [day_key, cnt].
                if (row.Length < 2)
                    continue;

                // Skip rows whose date cannot be parsed.
                if (!DateTime.TryParse(row[0]?.ToString(), out var day))
                    continue;

                // Count may come back as long/int/string depending on the MySQL provider.
                var raw = row[1]?.ToString();
                var count = 0;
                if (long.TryParse(raw, out var asLong))
                    count = (int)Math.Clamp(asLong, 0, int.MaxValue);
                else if (int.TryParse(raw, out var asInt))
                    count = Math.Max(0, asInt);

                // Store only the date part; chart logic does not need a time-of-day.
                result[day.Date] = count;
            }

            return result;
        }




    }
}
