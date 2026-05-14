// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;

namespace DBL
{
    public class StyleDB : BaseDB<Style>
    {
        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        protected override string GetPrimaryKeyName() => "style_id";
        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        protected override string GetTableName() => "eitan_project12.styles";

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        protected override async Task<Style> CreateModelAsync(object[] row)
        {
            // style_id, name, description
            var s = new Style
            {
                StyleId = row[0]?.ToString() ?? "",
                Name = row[1]?.ToString() ?? "",
                Description = row[2]?.ToString()
            };

            return await Task.FromResult(s);
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public Task<List<Style>> GetAllAsync()
        {
            return SelectAllAsync();
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<Style?> GetByIdAsync(string styleId)
        {
            var sql = "SELECT * FROM eitan_project12.styles WHERE style_id = @id";
            var list = await SelectAllAsync(sql,
                new Dictionary<string, object> { ["id"] = styleId });

            return list.FirstOrDefault();
        }

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        public Task<int> CreateAsync(Style s)
        {
            var fields = new Dictionary<string, object>
            {
                ["style_id"] = s.StyleId,
                ["name"] = s.Name,
                ["description"] = (object?)s.Description ?? DBNull.Value
            };

            return InsertAsync(fields);
        }
    }
}
