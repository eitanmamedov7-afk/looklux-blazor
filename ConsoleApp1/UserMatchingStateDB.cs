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
using System.Threading.Tasks;
using Models;

// הגדרת מרחו שמות שממקם את הקובץ בטבקת הפרויקט המטאימה.
namespace DBL
{
    // הגדרת מבנה מרכזי שמרכז נתונים או פעוליות עובר החלק הזה בפרויקט.
    public class UserMatchingStateDB : BaseDB<UserMatchingState>
    {
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        protected override string GetPrimaryKeyName() => "user_id";
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        protected override string GetTableName() => "eitan_project12.user_matching_state";

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        protected override Task<UserMatchingState> CreateModelAsync(object[] row)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var state = new UserMatchingState
            {
                UserId = row[0]?.ToString() ?? "",
                MinPerType = int.TryParse(row[1]?.ToString(), out var min) ? min : 5,
                LockedAfterFailure = Convert.ToInt32(row[2] ?? 0) != 0,
                LastFailureAt = DateTime.TryParse(row[3]?.ToString(), out var lf) ? lf : null,
                LastSuccessAt = DateTime.TryParse(row[4]?.ToString(), out var ls) ? ls : null
            };

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return Task.FromResult(state);
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public Task<UserMatchingState?> GetByUserAsync(string userId)
        {
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return SingleOrDefaultAsync(userId);
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        private async Task<UserMatchingState?> SingleOrDefaultAsync(string userId)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sql = $"SELECT * FROM {GetTableName()} WHERE user_id = @id";
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var list = await SelectAllAsync(sql, new Dictionary<string, object> { ["id"] = userId });
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return list.Count > 0 ? list[0] : null;
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<int> UpsertAsync(UserMatchingState state)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var fields = new Dictionary<string, object>
            {
                ["user_id"] = state.UserId,
                ["min_per_type"] = state.MinPerType,
                ["locked_after_failure"] = state.LockedAfterFailure ? 1 : 0,
                ["last_failure_at"] = state.LastFailureAt ?? (object)DBNull.Value,
                ["last_success_at"] = state.LastSuccessAt ?? (object)DBNull.Value
            };

            // קריאה לשכבת הדיבי כדי לשלוף, לשמור, לעדכן או למחוק מידע ששייך למשתמש.
            var insert = PrepareInsertQueryWithParameters(fields);
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var updatePart = "ON DUPLICATE KEY UPDATE min_per_type = VALUES(min_per_type), locked_after_failure = VALUES(locked_after_failure), last_failure_at = VALUES(last_failure_at), last_success_at = VALUES(last_success_at);";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sql = insert.Replace(";", " ") + updatePart;

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            return await ExecNonQueryAsync(sql);
        }
    }
}
