// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;

namespace DBL
{
    public class UserMatchingStateDB : BaseDB<UserMatchingState>
    {
        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        protected override string GetPrimaryKeyName() => "user_id";
        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        protected override string GetTableName() => "eitan_project12.user_matching_state";

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        protected override Task<UserMatchingState> CreateModelAsync(object[] row)
        {
            var state = new UserMatchingState
            {
                UserId = row[0]?.ToString() ?? "",
                MinPerType = int.TryParse(row[1]?.ToString(), out var min) ? min : 5,
                LockedAfterFailure = Convert.ToInt32(row[2] ?? 0) != 0,
                LastFailureAt = DateTime.TryParse(row[3]?.ToString(), out var lf) ? lf : null,
                LastSuccessAt = DateTime.TryParse(row[4]?.ToString(), out var ls) ? ls : null
            };

            return Task.FromResult(state);
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public Task<UserMatchingState?> GetByUserAsync(string userId)
        {
            return SingleOrDefaultAsync(userId);
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        private async Task<UserMatchingState?> SingleOrDefaultAsync(string userId)
        {
            var sql = $"SELECT * FROM {GetTableName()} WHERE user_id = @id";
            var list = await SelectAllAsync(sql, new Dictionary<string, object> { ["id"] = userId });
            return list.Count > 0 ? list[0] : null;
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public async Task<int> UpsertAsync(UserMatchingState state)
        {
            var fields = new Dictionary<string, object>
            {
                ["user_id"] = state.UserId,
                ["min_per_type"] = state.MinPerType,
                ["locked_after_failure"] = state.LockedAfterFailure ? 1 : 0,
                ["last_failure_at"] = state.LastFailureAt ?? (object)DBNull.Value,
                ["last_success_at"] = state.LastSuccessAt ?? (object)DBNull.Value
            };

            // MySQL upsert
            var insert = PrepareInsertQueryWithParameters(fields);
            var updatePart = "ON DUPLICATE KEY UPDATE min_per_type = VALUES(min_per_type), locked_after_failure = VALUES(locked_after_failure), last_failure_at = VALUES(last_failure_at), last_success_at = VALUES(last_success_at);";
            var sql = insert.Replace(";", " ") + updatePart;

            return await ExecNonQueryAsync(sql);
        }
    }
}
