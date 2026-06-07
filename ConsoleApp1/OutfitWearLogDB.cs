// SEARCH INDEX
// OUTFIT_WEAR_LOG, DATABASE, OUTFIT, ADD, LIST, HISTORY, TIMESTAMP, COUNT
//
// Topic: Outfit wear log database
// Purpose: Inserts and reads records for outfits that users/admins mark as worn.
// Search keywords: OUTFIT_WEAR_LOG DATABASE OUTFIT ADD LIST HISTORY TIMESTAMP COUNT
// When to use it: Show this when explaining how the project stores the time an outfit was worn.
// Important notes: This class only writes outfit wear history. It does not edit outfits or garments.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;

namespace DBL
{
    // SECTION: OUTFIT WEAR HISTORY DATABASE
    // Topic: OutfitWearLogDB class
    // Purpose: Table-specific DB logic for eitan_project12.outfit_wear_logs.
    // Search keywords: OUTFIT_WEAR_LOG DATABASE OUTFIT HISTORY ADD LIST
    // When to use it: Use when a customer or admin clicks "Mark worn" for a saved outfit.
    // Important notes: The SQL table has foreign keys to users and outfits, so logs are removed when those rows are deleted.
    public class OutfitWearLogDB : BaseDB<OutfitWearLog>
    {
        // BaseDB uses this primary key for generic helper methods.
        protected override string GetPrimaryKeyName() => "wear_log_id";

        // Real database table that stores outfit worn timestamps.
        protected override string GetTableName() => "eitan_project12.outfit_wear_logs";

        // Topic: Convert SQL row to model
        // Purpose: Builds the OutfitWearLog object used by the API/UI from a database row.
        // Search keywords: OUTFIT_WEAR_LOG DATABASE LIST MODEL
        // When to use it: Use when reading wear history from MySQL.
        // Important notes: Current UI mainly inserts logs, but this keeps the DB class complete and testable.
        protected override Task<OutfitWearLog> CreateModelAsync(object[] row)
        {
            return Task.FromResult(new OutfitWearLog
            {
                WearLogId = row[0]?.ToString() ?? string.Empty,
                UserId = row[1]?.ToString() ?? string.Empty,
                OutfitId = row[2]?.ToString() ?? string.Empty,
                WornAt = DateTime.TryParse(row[3]?.ToString(), out var wornAt) ? wornAt : DateTime.UtcNow
            });
        }

        // Topic: Mark outfit worn
        // Purpose: Inserts one timestamped record showing the selected outfit was worn.
        // Search keywords: OUTFIT_WEAR_LOG DATABASE OUTFIT ADD TIMESTAMP
        // When to use it: Call after the web page or MAUI app verifies the user/admin may access this outfit.
        // Important notes: DateTime.Now uses the server's current timestamp, matching the project database time zone.
        // FLOW_OUTFIT_WEAR_WEB_03: Web MarkOutfitWornAsync calls this method to insert the wear record.
        // This file is involved because the final web step is a MySQL insert; next step is the UI success message.
        // FLOW_OUTFIT_WEAR_MOBILE_05: Mobile API calls this method after access checks.
        // This file is involved because MAUI cannot write SQL directly; next step is Program.cs returning success JSON.
        public async Task<OutfitWearLog> MarkWornAsync(string userId, string outfitId)
        {
            var now = DateTime.Now;
            var log = new OutfitWearLog
            {
                WearLogId = Guid.NewGuid().ToString(),
                UserId = userId,
                OutfitId = outfitId,
                WornAt = now
            };

            await InsertAsync(new Dictionary<string, object>
            {
                ["wear_log_id"] = log.WearLogId,
                ["user_id"] = log.UserId,
                ["outfit_id"] = log.OutfitId,
                ["worn_at"] = log.WornAt
            });
            return log;
        }

        // Topic: List outfit wear history
        // Purpose: Reads recent wear records for one outfit.
        // Search keywords: OUTFIT_WEAR_LOG DATABASE OUTFIT LIST HISTORY
        // When to use it: Use later if the project needs to display a history such as "last worn".
        // Important notes: This is intentionally small and does not change the current outfit page behavior.
        public async Task<List<OutfitWearLog>> GetByOutfitAsync(string outfitId, int limit = 50)
        {
            var sql = $@"
SELECT wear_log_id, user_id, outfit_id, worn_at
FROM {GetTableName()}
WHERE outfit_id = @outfit_id
ORDER BY worn_at DESC
LIMIT {Math.Max(1, limit)}";

            return await SelectAllAsync(sql, new Dictionary<string, object>
            {
                ["outfit_id"] = outfitId
            });
        }

        // Topic: Outfit wear summary
        // Purpose: Counts wear records and finds first/last worn timestamps for many outfits in one query.
        // Search keywords: OUTFIT_WEAR_LOG DATABASE OUTFIT LIST HISTORY COUNT TIMESTAMP
        // When to use it: Call after loading outfit cards so web/API can show usage summary per outfit.
        // Important notes: Missing outfits simply have zero wears and no first/last timestamp.
        // FLOW_OUTFIT_WEAR_STATS_02: OutfitWearLogDB groups outfit_wear_logs by outfit_id.
        // This file is involved because MySQL can calculate count/min/max efficiently; next step is applying summaries to Outfit models.
        public async Task<Dictionary<string, OutfitWearLog>> GetSummariesByOutfitIdsAsync(IEnumerable<string> outfitIds)
        {
            var ids = outfitIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (ids.Count == 0)
                return new Dictionary<string, OutfitWearLog>(StringComparer.OrdinalIgnoreCase);

            var placeholders = string.Join(",", ids.Select((_, i) => $"@id{i}"));
            var parameters = new Dictionary<string, object>();
            for (var i = 0; i < ids.Count; i++)
                parameters[$"id{i}"] = ids[i];

            var sql = $@"
SELECT outfit_id, COUNT(*) AS wear_count, MIN(worn_at) AS first_worn_at, MAX(worn_at) AS last_worn_at
FROM {GetTableName()}
WHERE outfit_id IN ({placeholders})
GROUP BY outfit_id";

            var rows = await StringListSelectAllAsync(sql, parameters);
            var summaries = new Dictionary<string, OutfitWearLog>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                var outfitId = row[0]?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(outfitId))
                    continue;

                var firstWornAt = DateTime.TryParse(row[2]?.ToString(), out var first) ? first : (DateTime?)null;
                var lastWornAt = DateTime.TryParse(row[3]?.ToString(), out var last) ? last : (DateTime?)null;

                summaries[outfitId] = new OutfitWearLog
                {
                    OutfitId = outfitId,
                    WearCount = int.TryParse(row[1]?.ToString(), out var count) ? count : 0,
                    FirstWornAt = firstWornAt,
                    LastWornAt = lastWornAt,
                    WornAt = lastWornAt ?? DateTime.UtcNow
                };
            }

            return summaries;
        }
    }
}
