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
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

// הגדרת מרחו שמות שממקם את הקובץ בטבקת הפרויקט המטאימה.
namespace DBL
{
    // הגדרת מבנה מרכזי שמרכז נתונים או פעוליות עובר החלק הזה בפרויקט.
    public abstract class BaseDB<T> : DB
    {
        protected abstract string GetTableName();
        protected abstract string GetPrimaryKeyName();
        protected abstract Task<T> CreateModelAsync(object[] row);

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        protected virtual async Task<List<T>> CreateListModelAsync(List<object[]> rows)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var list = new List<T>();
            // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
            foreach (var row in rows)
            {
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                var dto = await CreateModelAsync(row);
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (dto != null)
                    list.Add(dto);
            }
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return list;
        }


        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public Task<List<T>> SelectAllAsync() =>
            SelectAllAsync("", new Dictionary<string, object>());

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public Task<List<T>> SelectAllAsync(Dictionary<string, object> parameters) =>
            SelectAllAsync("", parameters);

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public Task<List<T>> SelectAllAsync(string query) =>
            SelectAllAsync(query, new Dictionary<string, object>());

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<List<T>> SelectAllAsync(string query, Dictionary<string, object> parameters)
        {
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            var list = await StringListSelectAllAsync(query, parameters);
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            return await CreateListModelAsync(list);
        }


        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<int> InsertAsync(Dictionary<string, object> keyAndValue)
        {
            // קריאה לשכבת הדיבי כדי לשלוף, לשמור, לעדכן או למחוק מידע ששייך למשתמש.
            var sqlCommand = PrepareInsertQueryWithParameters(keyAndValue);
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            return await ExecNonQueryAsync(sqlCommand);
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<object?> InsertGetObjAsync(Dictionary<string, object> keyAndValue)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var pkName = GetPrimaryKeyName();
            object? providedPk = null;

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (keyAndValue != null && keyAndValue.TryGetValue(pkName, out var pkVal) && pkVal != null)
                providedPk = pkVal;

            // קריאה לשכבת הדיבי כדי לשלוף, לשמור, לעדכן או למחוק מידע ששייך למשתמש.
            var sqlCommand = PrepareInsertQueryWithParameters(keyAndValue);

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (providedPk != null)
            {
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                var affected = await ExecNonQueryAsync(sqlCommand);
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (affected <= 0) return null;

                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var p = new Dictionary<string, object> { { "id", providedPk } };
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var sql = $@"SELECT * FROM {GetTableName()} WHERE ({pkName} = @id)";
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                var list = await SelectAllAsync(sql, p);
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return (list.Count == 1) ? list[0] : null;
            }
            // מסלול חלופי שפועל כאשר התנאי הקודם לא התקיים.
            else
            {
                sqlCommand += " SELECT LAST_INSERT_ID();";
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                var res = await ExecScalarAsync(sqlCommand);
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (res == null) return null;

                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var p = new Dictionary<string, object> { { "id", res.ToString()! } };
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var sql = $@"SELECT * FROM {GetTableName()} WHERE ({pkName} = @id)";
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                var list = await SelectAllAsync(sql, p);
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return (list.Count == 1) ? list[0] : null;
            }
        }


        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<int> UpdateAsync(Dictionary<string, object> fieldValue, Dictionary<string, object> parameters)
        {
            // קריאה לשכבת הדיבי כדי לשלוף, לשמור, לעדכן או למחוק מידע ששייך למשתמש.
            var where = PrepareWhereQueryWithParameters(parameters);
            // קריאה לשכבת הדיבי כדי לשלוף, לשמור, לעדכן או למחוק מידע ששייך למשתמש.
            var inKeyValue = PrepareUpdateQueryWithParameters(fieldValue);
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (string.IsNullOrWhiteSpace(inKeyValue))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return 0;

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sqlCommand = $"UPDATE {GetTableName()} SET {inKeyValue} {where}";
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            return await ExecNonQueryAsync(sqlCommand);
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<int> DeleteAsync(Dictionary<string, object> parameters)
        {
            // קריאה לשכבת הדיבי כדי לשלוף, לשמור, לעדכן או למחוק מידע ששייך למשתמש.
            var where = PrepareWhereQueryWithParameters(parameters);
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sqlCommand = $"DELETE FROM {GetTableName()} {where}";
            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            return await ExecNonQueryAsync(sqlCommand);
        }


        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public void AddParameterToCommand(string name, object? value)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task PreQueryAsync(string query)
        {
            cmd.CommandText = query;
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (cmd.Connection == null)
                cmd.Connection = conn;

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (conn.State != System.Data.ConnectionState.Open)
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                await conn.OpenAsync();

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (cmd.Connection.State != System.Data.ConnectionState.Open)
                cmd.Connection = conn;
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task PostQueryAsync()
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (reader != null && !reader.IsClosed)
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                await reader.CloseAsync();

            cmd.Parameters.Clear();

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (conn.State == System.Data.ConnectionState.Open)
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                await conn.CloseAsync();
        }


        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public string PrepareWhereQueryWithParameters(Dictionary<string, object>? parameters)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (parameters == null || parameters.Count == 0)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return "";

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var where = "WHERE ";
            const string AND = "AND";
            // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
            foreach (var kv in parameters)
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var prm = $"@W{kv.Key}";
                where += $"{kv.Key}={prm} {AND} ";
                AddParameterToCommand(prm, kv.Value);
            }
            where = where.Remove(where.Length - AND.Length - 2);
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return where;
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public string PrepareUpdateQueryWithParameters(Dictionary<string, object>? fields)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (fields == null || fields.Count == 0)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return "";

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var parts = new List<string>();
            // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
            foreach (var kv in fields)
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var prm = $"@{kv.Key}";
                parts.Add($"{kv.Key}={prm}");
                AddParameterToCommand(prm, kv.Value);
            }
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return string.Join(",", parts);
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public string PrepareInsertQueryWithParameters(Dictionary<string, object> fields)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (fields == null || fields.Count == 0)
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return "";

            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var inKey = "(" + string.Join(",", fields.Keys) + ")";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var placeholders = new List<string>();
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            int i = 0;
            // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
            foreach (var val in fields.Values)
            {
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var pn = "@" + i++;
                placeholders.Add(pn);
                AddParameterToCommand(pn, val);
            }
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var inValue = "VALUES(" + string.Join(",", placeholders) + ")";
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var sqlCommand = $"INSERT INTO {GetTableName()} {inKey} {inValue};";
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return sqlCommand;
        }


        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<int> ExecNonQueryAsync(string query)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (string.IsNullOrWhiteSpace(query))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return 0;

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            await PreQueryAsync(query);
            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                return await cmd.ExecuteNonQueryAsync();
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + "\nsql:" + cmd.CommandText);
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return 0;
            }
            // ניקוי או סגירת משאבים שסרוצים בסוף הפעולה.
            finally
            {
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                await PostQueryAsync();
            }
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<object?> ExecScalarAsync(string query)
        {
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (string.IsNullOrWhiteSpace(query))
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return null;

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            await PreQueryAsync(query);
            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                return await cmd.ExecuteScalarAsync();
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + "\nsql:" + cmd.CommandText);
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return null;
            }
            // ניקוי או סגירת משאבים שסרוצים בסוף הפעולה.
            finally
            {
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                await PostQueryAsync();
            }
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<List<object[]>> StringListSelectAllAsync(string query, Dictionary<string, object> parameters)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var list = new List<object[]>();
            string sqlCommand;

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (string.IsNullOrWhiteSpace(query))
            {
                // קריאה לשכבת הדיבי כדי לשלוף, לשמור, לעדכן או למחוק מידע ששייך למשתמש.
                var where = PrepareWhereQueryWithParameters(parameters);
                sqlCommand = $"SELECT * FROM {GetTableName()} {where}";
            }
            // מסלול חלופי שפועל כאשר התנאי הקודם לא התקיים.
            else if (query.Contains("@"))
            {
                sqlCommand = query;
                // לולאה שמבצעת את אותה פעולה עבור כל פריט ברשימה או כל עוד התנאי מתקיים.
                foreach (var kv in parameters)
                    AddParameterToCommand("@" + kv.Key, kv.Value);
            }
            // מסלול חלופי שפועל כאשר התנאי הקודם לא התקיים.
            else
            {
                // קריאה לשכבת הדיבי כדי לשלוף, לשמור, לעדכן או למחוק מידע ששייך למשתמש.
                var where = PrepareWhereQueryWithParameters(parameters);
                sqlCommand = $"{query} {where}";
            }

            // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
            await PreQueryAsync(sqlCommand);
            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                reader = await cmd.ExecuteReaderAsync();
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                var cols = await reader.GetColumnSchemaAsync();
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var size = cols.Count;

                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                while (await reader.ReadAsync())
                {
                    // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                    var row = new object[size];
                    reader.GetValues(row);
                    list.Add(row);
                }
            }
            // טיפול בשגיאה כדי להחזיר תוצאה בטוחה במקום קריסה.
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + "\nsql:" + cmd.CommandText);
                list.Clear();
            }
            // ניקוי או סגירת משאבים שסרוצים בסוף הפעולה.
            finally
            {
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                await PostQueryAsync();
            }

            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return list;
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public Task<List<object[]>> StingListSelectAllAsync(string query, Dictionary<string, object> parameters)
            => StringListSelectAllAsync(query, parameters);
    }
}
