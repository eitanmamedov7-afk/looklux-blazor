// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using MySql.Data.MySqlClient;

namespace DBL
{
    public class OutfitGarmentDB : BaseDB<OutfitGarment>
    {
        private HashSet<string>? _columnCache;

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        protected override string GetPrimaryKeyName() => "outfit_garment_id";
        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        protected override string GetTableName() => "eitan_project12.outfit_garments";

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        protected override Task<OutfitGarment> CreateModelAsync(object[] row)
        {
            // New schema:
            // 0 outfit_garment_id, 1 outfit_id, 2 garment_id, 3 garment_type, 4 is_seed, 5 created_at
            if (row.Length >= 6)
            {
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

            // Legacy schema:
            // 0 outfit_id, 1 garment_id, 2 role
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

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        public async Task<int> CreateAsync(OutfitGarment item)
        {
            var columns = await GetColumnSetAsync();
            var hasRole = columns.Contains("role");
            var hasNewColumns = columns.Contains("outfit_garment_id") && columns.Contains("garment_type");

            if (hasRole && !hasNewColumns)
            {
                var legacyFields = new Dictionary<string, object>
                {
                    ["outfit_id"] = item.OutfitId,
                    ["garment_id"] = item.GarmentId,
                    ["role"] = NormalizeRole(item.GarmentType)
                };

                return await InsertAsync(legacyFields);
            }

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

            return await InsertAsync(fields);
        }

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        public async Task<bool> CreateForOutfitAsync(string outfitId, IEnumerable<OutfitGarment> rows)
        {
            var allOk = true;
            foreach (var row in rows)
            {
                row.OutfitId = outfitId;
                if (row.CreatedAt == default) row.CreatedAt = DateTime.UtcNow;

                var inserted = await CreateAsync(row);
                if (inserted <= 0)
                    allOk = false;
            }

            return allOk;
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        public async Task<List<OutfitGarment>> GetByOutfitIdsAsync(IEnumerable<string> outfitIds)
        {
            var ids = outfitIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (ids.Count == 0)
                return new List<OutfitGarment>();

            var columns = await GetColumnSetAsync();
            var hasRole = columns.Contains("role");
            var hasNewColumns = columns.Contains("outfit_garment_id") && columns.Contains("garment_type");

            var placeholders = string.Join(",", ids.Select((_, i) => $"@id{i}"));
            var parameters = new Dictionary<string, object>();
            for (int i = 0; i < ids.Count; i++)
                parameters[$"id{i}"] = ids[i];

            string sql;
            if (hasRole && !hasNewColumns)
            {
                sql = $@"SELECT outfit_id, garment_id, role
                         FROM {GetTableName()}
                         WHERE outfit_id IN ({placeholders})";
            }
            else
            {
                sql = $@"SELECT outfit_garment_id, outfit_id, garment_id, garment_type, is_seed, created_at
                         FROM {GetTableName()}
                         WHERE outfit_id IN ({placeholders})";
            }

            return await SelectAllAsync(sql, parameters);
        }

        // הסבר: פונקציית מחיקה. מסירה נתון קיים ומחזירה תוצאה כדי לאשר שהפעולה הושלמה.
        public async Task<int> DeleteByOutfitIdAsync(string outfitId)
        {
            if (string.IsNullOrWhiteSpace(outfitId))
                return 0;

            return await DeleteAsync(new Dictionary<string, object>
            {
                ["outfit_id"] = outfitId.Trim()
            });
        }

        // הסבר: פונקציית שליפה. מחזירה נתונים מה־DB/שירות בהתאם לפרמטרים שנשלחו.
        private async Task<HashSet<string>> GetColumnSetAsync()
        {
            if (_columnCache != null)
                return _columnCache;

            var sql = @"SELECT LOWER(column_name)
                        FROM information_schema.columns
                        WHERE table_schema = 'eitan_project12'
                          AND table_name = 'outfit_garments'";

            var rows = await StingListSelectAllAsync(sql, new Dictionary<string, object>());
            _columnCache = rows
                .Select(r => r.Length > 0 ? r[0]?.ToString() ?? string.Empty : string.Empty)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim().ToLowerInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return _columnCache;
        }

        // הסבר: פונקציית נרמול. מנקה ומאחד פורמט נתונים כדי למנוע חוסר עקביות בהמשך הזרימה.
        private static string NormalizeRole(string? raw)
        {
            var value = (raw ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            if (value.Contains("shirt") || value.Contains("tee") || value.Contains("top"))
                return "shirt";
            if (value.Contains("pant") || value.Contains("trouser") || value.Contains("jean") || value.Contains("bottom"))
                return "pants";
            if (value.Contains("shoe") || value.Contains("sneaker") || value.Contains("boot") || value.Contains("foot"))
                return "shoes";

            return value;
        }
    }
}
