// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace DBL
{
    // Version 3.5 — safe for UUID PKs, compatible with existing DB helpers
    public abstract class BaseDB<T> : DB
    {
        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        protected abstract string GetTableName();
        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        protected abstract string GetPrimaryKeyName();
        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        protected abstract Task<T> CreateModelAsync(object[] row);

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        protected virtual async Task<List<T>> CreateListModelAsync(List<object[]> rows)
        {
            var list = new List<T>();
            foreach (var row in rows)
            {
                var dto = await CreateModelAsync(row);
                if (dto != null)
                    list.Add(dto);
            }
            return list;
        }

        // -------------------------- SELECT --------------------------

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public Task<List<T>> SelectAllAsync() =>
            SelectAllAsync("", new Dictionary<string, object>());

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public Task<List<T>> SelectAllAsync(Dictionary<string, object> parameters) =>
            SelectAllAsync("", parameters);

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public Task<List<T>> SelectAllAsync(string query) =>
            SelectAllAsync(query, new Dictionary<string, object>());

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public async Task<List<T>> SelectAllAsync(string query, Dictionary<string, object> parameters)
        {
            var list = await StringListSelectAllAsync(query, parameters);
            return await CreateListModelAsync(list);
        }

        // -------------------------- INSERT --------------------------

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        public async Task<int> InsertAsync(Dictionary<string, object> keyAndValue)
        {
            var sqlCommand = PrepareInsertQueryWithParameters(keyAndValue);
            return await ExecNonQueryAsync(sqlCommand);
        }

        /// <summary>
        /// Insert + return inserted object.
        /// אם ה-PK סופק בשדות, נשתמש בו (מתאים ל-UUID).
        /// אחרת ננסה LAST_INSERT_ID() (לטבלאות עם AUTO_INCREMENT).
        /// </summary>
        public async Task<object?> InsertGetObjAsync(Dictionary<string, object> keyAndValue)
        {
            var pkName = GetPrimaryKeyName();
            object? providedPk = null;

            // אם סופק PK (UUID אצלנו) נשתמש בו לשחזור
            if (keyAndValue != null && keyAndValue.TryGetValue(pkName, out var pkVal) && pkVal != null)
                providedPk = pkVal;

            var sqlCommand = PrepareInsertQueryWithParameters(keyAndValue);

            if (providedPk != null)
            {
                // הוספה רגילה
                var affected = await ExecNonQueryAsync(sqlCommand);
                if (affected <= 0) return null;

                var p = new Dictionary<string, object> { { "id", providedPk } };
                var sql = $@"SELECT * FROM {GetTableName()} WHERE ({pkName} = @id)";
                var list = await SelectAllAsync(sql, p);
                return (list.Count == 1) ? list[0] : null;
            }
            else
            {
                // fallback למקרה של AUTO_INCREMENT
                sqlCommand += " SELECT LAST_INSERT_ID();";
                var res = await ExecScalarAsync(sqlCommand);
                if (res == null) return null;

                var p = new Dictionary<string, object> { { "id", res.ToString()! } };
                var sql = $@"SELECT * FROM {GetTableName()} WHERE ({pkName} = @id)";
                var list = await SelectAllAsync(sql, p);
                return (list.Count == 1) ? list[0] : null;
            }
        }

        // -------------------------- UPDATE / DELETE --------------------------

        // הסבר: פונקציית עדכון. משנה נתון קיים ושומרת את השינוי בצורה בטוחה.
        public async Task<int> UpdateAsync(Dictionary<string, object> fieldValue, Dictionary<string, object> parameters)
        {
            var where = PrepareWhereQueryWithParameters(parameters);
            var inKeyValue = PrepareUpdateQueryWithParameters(fieldValue);
            if (string.IsNullOrWhiteSpace(inKeyValue))
                return 0;

            var sqlCommand = $"UPDATE {GetTableName()} SET {inKeyValue} {where}";
            return await ExecNonQueryAsync(sqlCommand);
        }

        // הסבר: פונקציית מחיקה. מסירה נתון קיים ומחזירה תוצאה כדי לאשר שהפעולה הושלמה.
        public async Task<int> DeleteAsync(Dictionary<string, object> parameters)
        {
            var where = PrepareWhereQueryWithParameters(parameters);
            var sqlCommand = $"DELETE FROM {GetTableName()} {where}";
            return await ExecNonQueryAsync(sqlCommand);
        }

        // -------------------------- PARAMS / PRE/POST --------------------------

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        public void AddParameterToCommand(string name, object? value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public async Task PreQueryAsync(string query)
        {
            cmd.CommandText = query;
            if (cmd.Connection == null)
                cmd.Connection = conn;

            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            if (cmd.Connection.State != System.Data.ConnectionState.Open)
                cmd.Connection = conn;
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public async Task PostQueryAsync()
        {
            if (reader != null && !reader.IsClosed)
                await reader.CloseAsync();

            cmd.Parameters.Clear();

            if (conn.State == System.Data.ConnectionState.Open)
                await conn.CloseAsync();
        }

        // -------------------------- BUILDERS --------------------------

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public string PrepareWhereQueryWithParameters(Dictionary<string, object>? parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return "";

            var where = "WHERE ";
            const string AND = "AND";
            foreach (var kv in parameters)
            {
                var prm = $"@W{kv.Key}";
                where += $"{kv.Key}={prm} {AND} ";
                AddParameterToCommand(prm, kv.Value);
            }
            where = where.Remove(where.Length - AND.Length - 2); // remove last ' AND '
            return where;
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public string PrepareUpdateQueryWithParameters(Dictionary<string, object>? fields)
        {
            if (fields == null || fields.Count == 0)
                return "";

            var parts = new List<string>();
            foreach (var kv in fields)
            {
                var prm = $"@{kv.Key}";
                parts.Add($"{kv.Key}={prm}");
                AddParameterToCommand(prm, kv.Value);
            }
            return string.Join(",", parts);
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public string PrepareInsertQueryWithParameters(Dictionary<string, object> fields)
        {
            if (fields == null || fields.Count == 0)
                return "";

            var inKey = "(" + string.Join(",", fields.Keys) + ")";
            var placeholders = new List<string>();
            int i = 0;
            foreach (var val in fields.Values)
            {
                var pn = "@" + i++;
                placeholders.Add(pn);
                AddParameterToCommand(pn, val);
            }
            var inValue = "VALUES(" + string.Join(",", placeholders) + ")";
            var sqlCommand = $"INSERT INTO {GetTableName()} {inKey} {inValue};";
            return sqlCommand;
        }

        // -------------------------- EXEC --------------------------

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public async Task<int> ExecNonQueryAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return 0;

            await PreQueryAsync(query);
            try
            {
                return await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + "\nsql:" + cmd.CommandText);
                return 0;
            }
            finally
            {
                await PostQueryAsync();
            }
        }

        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public async Task<object?> ExecScalarAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return null;

            await PreQueryAsync(query);
            try
            {
                return await cmd.ExecuteScalarAsync();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + "\nsql:" + cmd.CommandText);
                return null;
            }
            finally
            {
                await PostQueryAsync();
            }
        }

        /// <summary>
        /// בחירת נתונים כללי עם פרמטרים. תומך ב-3 מצבים:
        /// 1) query ריק → SELECT * FROM table WHERE ...
        /// 2) query כולל @ → נשתמש ב-query כמו שהוא ונוסיף פרמטרים
        /// 3) אחרת → נשרשר WHERE שנבנה מה-parameters לסוף ה-query
        /// </summary>
        public async Task<List<object[]>> StringListSelectAllAsync(string query, Dictionary<string, object> parameters)
        {
            var list = new List<object[]>();
            string sqlCommand;

            if (string.IsNullOrWhiteSpace(query))
            {
                var where = PrepareWhereQueryWithParameters(parameters);
                sqlCommand = $"SELECT * FROM {GetTableName()} {where}";
            }
            else if (query.Contains("@"))
            {
                sqlCommand = query;
                foreach (var kv in parameters)
                    AddParameterToCommand("@" + kv.Key, kv.Value);
            }
            else
            {
                var where = PrepareWhereQueryWithParameters(parameters);
                sqlCommand = $"{query} {where}";
            }

            await PreQueryAsync(sqlCommand);
            try
            {
                reader = await cmd.ExecuteReaderAsync();
                var cols = await reader.GetColumnSchemaAsync();
                var size = cols.Count;

                while (await reader.ReadAsync())
                {
                    var row = new object[size];
                    reader.GetValues(row);
                    list.Add(row);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + "\nsql:" + cmd.CommandText);
                list.Clear();
            }
            finally
            {
                await PostQueryAsync();
            }

            return list;
        }

        // תאימות לאחור לשם השגוי "StingListSelectAllAsync"
        // הסבר: פונקציה זו היא חלק מזרימת הקובץ: קלט -> עיבוד -> תוצאה/עדכון מצב.
        public Task<List<object[]>> StingListSelectAllAsync(string query, Dictionary<string, object> parameters)
            => StringListSelectAllAsync(query, parameters);
    }
}
