
// SEARCH INDEX
// DATABASE, BASE, SELECT, ADD, UPDATE, REMOVE, QUERY, PARAMETER, LIST, SEARCH
//
// Topic: GENERIC DATABASE HELPER
// Purpose: Reusable SQL helper for selecting, inserting, updating, deleting, and mapping rows to models.
// Search keywords: DATABASE BASE SELECT ADD UPDATE REMOVE QUERY PARAMETER LIST SEARCH
// When to use it: Show this when explaining the common DB pattern used by UserDB/GarmentDB/OutfitDB.
// Important notes: Child classes provide table name, primary key name, and row-to-model mapping.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace DBL
{
    // SECTION: DATABASE BASE CRUD
    // Topic: BaseDB generic class
    // Purpose: Centralizes repeated SQL CRUD code so each table DB class stays focused on table-specific queries.
    // Search keywords: DATABASE BASE CRUD SELECT ADD UPDATE REMOVE LIST
    // When to use it: Use when explaining why DB classes inherit from BaseDB<T>.
    // Important notes: BaseDB executes SQL; child DB classes decide which SQL and how to map rows.
    // Generic database helper used by the project-specific DB classes.
    // UserDB, GarmentDB, OutfitDB, and OutfitGarmentDB use this class for common SELECT/INSERT/UPDATE/DELETE work.
    // The child class tells BaseDB which table it uses and how to convert SQL rows into model objects.
    public abstract class BaseDB<T> : DB
    {
        // Child DB class supplies the real table name, for example eitan_project12.outfits.
        protected abstract string GetTableName();

        // Child DB class supplies the primary key column used by generic helpers.
        protected abstract string GetPrimaryKeyName();

        // Child DB class converts one raw SQL row into the correct model type.
        // This is why BaseDB can be shared while each table still creates its own model.
        protected abstract Task<T> CreateModelAsync(object[] row);

        // Converts many raw SQL rows into a list of models.
        // This affects all screens/API endpoints that load lists from the database.
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


        // Topic: Generic select overloads
        // Purpose: Convenience entry points for loading table rows with or without custom SQL/parameters.
        // Search keywords: SELECT LIST SEARCH DATABASE
        // When to use it: Use when a DB class needs rows mapped into model objects.
        // Important notes: These methods route into the main SelectAllAsync overload below.
        public Task<List<T>> SelectAllAsync() =>
            SelectAllAsync("", new Dictionary<string, object>());

        public Task<List<T>> SelectAllAsync(Dictionary<string, object> parameters) =>
            SelectAllAsync("", parameters);

        public Task<List<T>> SelectAllAsync(string query) =>
            SelectAllAsync(query, new Dictionary<string, object>());

        // Main typed SELECT helper.
        // It runs SQL, gets raw rows, then maps them through CreateModelAsync.
        public async Task<List<T>> SelectAllAsync(string query, Dictionary<string, object> parameters)
        {
            var list = await StringListSelectAllAsync(query, parameters);
            return await CreateListModelAsync(list);
        }


        // Generic INSERT helper used when a child DB class passes column/value pairs.
        // This affects create flows such as new users, garments, outfits, and outfit links.
        // Topic: Generic insert
        // Purpose: Builds an INSERT statement from a dictionary of column/value pairs.
        // Search keywords: ADD INSERT DATABASE PARAMETER
        // When to use it: Use for create flows such as new users, garments, and outfits.
        // Important notes: Values must be supplied as parameters to avoid hand-built SQL values.
        public async Task<int> InsertAsync(Dictionary<string, object> keyAndValue)
        {
            var sqlCommand = PrepareInsertQueryWithParameters(keyAndValue);
            return await ExecNonQueryAsync(sqlCommand);
        }

        // Inserts a row and returns the inserted model when possible.
        // If the caller supplied its own primary key, it reloads by that key; otherwise it uses LAST_INSERT_ID().
        public async Task<object?> InsertGetObjAsync(Dictionary<string, object> keyAndValue)
        {
            var pkName = GetPrimaryKeyName();
            object? providedPk = null;

            if (keyAndValue != null && keyAndValue.TryGetValue(pkName, out var pkVal) && pkVal != null)
                providedPk = pkVal;

            var sqlCommand = PrepareInsertQueryWithParameters(keyAndValue);

            if (providedPk != null)
            {
                var affected = await ExecNonQueryAsync(sqlCommand);
                if (affected <= 0) return null;

                var p = new Dictionary<string, object> { { "id", providedPk } };
                var sql = $@"SELECT * FROM {GetTableName()} WHERE ({pkName} = @id)";
                var list = await SelectAllAsync(sql, p);
                return (list.Count == 1) ? list[0] : null;
            }
            else
            {
                sqlCommand += " SELECT LAST_INSERT_ID();";
                var res = await ExecScalarAsync(sqlCommand);
                if (res == null) return null;

                var p = new Dictionary<string, object> { { "id", res.ToString()! } };
                var sql = $@"SELECT * FROM {GetTableName()} WHERE ({pkName} = @id)";
                var list = await SelectAllAsync(sql, p);
                return (list.Count == 1) ? list[0] : null;
            }
        }


        // Generic UPDATE helper.
        // Child DB classes pass fields to change and WHERE parameters to choose the row.
        // Topic: Generic update
        // Purpose: Builds an UPDATE statement from changed fields and WHERE parameters.
        // Search keywords: UPDATE DATABASE PARAMETER VALIDATE
        // When to use it: Use when editing users/passwords or other saved rows.
        // Important notes: Returns affected row count so callers know whether anything changed.
        public async Task<int> UpdateAsync(Dictionary<string, object> fieldValue, Dictionary<string, object> parameters)
        {
            var where = PrepareWhereQueryWithParameters(parameters);
            var inKeyValue = PrepareUpdateQueryWithParameters(fieldValue);
            if (string.IsNullOrWhiteSpace(inKeyValue))
                return 0;

            var sqlCommand = $"UPDATE {GetTableName()} SET {inKeyValue} {where}";
            return await ExecNonQueryAsync(sqlCommand);
        }

        // Generic DELETE helper.
        // Child DB classes pass the WHERE fields so only the intended row or related rows are deleted.
        // Topic: Generic delete
        // Purpose: Deletes rows matching the supplied WHERE parameters.
        // Search keywords: REMOVE DELETE DATABASE PARAMETER
        // When to use it: Use for delete flows after callers confirm the operation is allowed.
        // Important notes: Child DB classes decide which table and WHERE fields are safe.
        public async Task<int> DeleteAsync(Dictionary<string, object> parameters)
        {
            var where = PrepareWhereQueryWithParameters(parameters);
            var sqlCommand = $"DELETE FROM {GetTableName()} {where}";
            return await ExecNonQueryAsync(sqlCommand);
        }


        // Adds one SQL parameter to the shared command object.
        // This keeps values out of raw SQL text and protects queries from SQL injection.
        public void AddParameterToCommand(string name, object? value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        // Prepares a command before execution.
        // It sets the SQL text and opens the MySQL connection if needed.
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

        // Cleans up after a query.
        // It closes the reader, clears parameters, and closes the connection so the next query starts clean.
        public async Task PostQueryAsync()
        {
            if (reader != null && !reader.IsClosed)
                await reader.CloseAsync();

            cmd.Parameters.Clear();

            if (conn.State == System.Data.ConnectionState.Open)
                await conn.CloseAsync();
        }


        // Builds a WHERE clause from parameter names and values.
        // Example: { garment_id = "x" } becomes WHERE garment_id=@Wgarment_id.
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
            where = where.Remove(where.Length - AND.Length - 2);
            return where;
        }

        // Builds the SET part of an UPDATE query from field/value pairs.
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

        // Builds an INSERT query from field/value pairs.
        // Values are added as parameters so the SQL stays safe.
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


        // Executes INSERT, UPDATE, or DELETE SQL and returns the affected row count.
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

        // Executes SQL that returns one value, for example LAST_INSERT_ID().
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

        // Main raw SELECT helper.
        // It supports default SELECT * queries, custom SQL with parameters, and custom SQL with generated WHERE clauses.
        // The result stays as object[] rows so child DB classes can map the row shape they selected.
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

        // Backward-compatible alias kept because existing DB classes call the misspelled name.
        public Task<List<object[]>> StingListSelectAllAsync(string query, Dictionary<string, object> parameters)
            => StringListSelectAllAsync(query, parameters);
    }
}
