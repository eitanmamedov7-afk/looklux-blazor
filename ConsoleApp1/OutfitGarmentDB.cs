



using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;

namespace DBL
{
    // Data-access class for the outfit_garments link table.
    // Each saved outfit is connected to its three garment rows here:
    // one shirt, one pants item, and one shoes item. MatchingService writes these rows
    // when an outfit suggestion is saved, and the Outfits/Admin pages read or delete them.
    // Current DB columns are: outfit_id, garment_id, role.
    public class OutfitGarmentDB : BaseDB<OutfitGarment>
    {
        // BaseDB requires a primary-key name, but this table is a link table in the current DB.
        // This class does not use BaseDB primary-key helpers for outfit_garments.
        protected override string GetPrimaryKeyName() => "outfit_id";

        // The database table that stores outfit-to-garment links.
        protected override string GetTableName() => "eitan_project12.outfit_garments";

        // Converts one outfit_garments database row into an OutfitGarment model.
        // Expected column order:
        // outfit_id, garment_id, role.
        protected override Task<OutfitGarment> CreateModelAsync(object[] row)
        {
            // Map the selected columns into the model used by outfit pages and services.
            return Task.FromResult(new OutfitGarment
            {
                OutfitGarmentId = string.Empty,
                OutfitId = row[0]?.ToString() ?? string.Empty,
                GarmentId = row[1]?.ToString() ?? string.Empty,
                GarmentType = NormalizeGarmentType(row[2]?.ToString()),
                IsSeed = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Inserts one link row between an outfit and one garment.
        // The role value is normalized so matching/filtering always sees shirt/pants/shoes.
        public Task<int> CreateAsync(OutfitGarment item)
        {
            // Field names match the current outfit_garments table columns.
            var fields = new Dictionary<string, object>
            {
                ["outfit_id"] = item.OutfitId,
                ["garment_id"] = item.GarmentId,
                ["role"] = NormalizeGarmentType(item.GarmentType)
            };

            // BaseDB builds and executes the INSERT.
            return InsertAsync(fields);
        }

        // Saves all garment links for one outfit.
        // MatchingService passes the three rows after it creates an outfit recommendation.
        public async Task<bool> CreateForOutfitAsync(string outfitId, IEnumerable<OutfitGarment> rows)
        {
            // Insert each shirt/pants/shoes link for the saved outfit.
            foreach (var row in rows)
            {
                // Force every row to point at the outfit created by the caller.
                row.OutfitId = outfitId;

                // Stop on first failed insert so save flow can report failure.
                var inserted = await CreateAsync(row);
                if (inserted <= 0)
                    return false;
            }

            return true;
        }

        // Loads garment links for a set of outfit ids.
        // Outfits.razor uses this to know which garments belong to each saved outfit.
        public async Task<List<OutfitGarment>> GetByOutfitIdsAsync(IEnumerable<string> outfitIds)
        {
            // Clean input ids and remove duplicates before building the IN query.
            var ids = outfitIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // No ids means there is nothing to load.
            if (ids.Count == 0)
                return new List<OutfitGarment>();

            // Build one SQL placeholder per outfit id: @id0, @id1, ...
            var placeholders = string.Join(",", ids.Select((_, i) => $"@id{i}"));
            // Parameters hold the actual ids separately from the SQL text.
            var parameters = new Dictionary<string, object>();
            for (int i = 0; i < ids.Count; i++)
                parameters[$"id{i}"] = ids[i];

            // Select columns in the same order expected by CreateModelAsync.
            var sql = $@"SELECT outfit_id, garment_id, role
                         FROM {GetTableName()}
                         WHERE outfit_id IN ({placeholders})";

            // BaseDB runs the query and maps rows to OutfitGarment objects.
            return await SelectAllAsync(sql, parameters);
        }

        // Deletes every garment link for one outfit.
        // Admin and Outfits pages call this before deleting the outfit row.
        public async Task<int> DeleteByOutfitIdAsync(string outfitId)
        {
            // Empty id is ignored to avoid deleting links accidentally.
            if (string.IsNullOrWhiteSpace(outfitId))
                return 0;

            // Delete only rows that belong to this outfit.
            return await DeleteAsync(new Dictionary<string, object>
            {
                ["outfit_id"] = outfitId.Trim()
            });
        }

        // Keeps type names consistent even if callers pass labels like "top" or "sneaker".
        private static string NormalizeGarmentType(string? raw)
        {
            // Normalize input once before checking known aliases.
            var value = (raw ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            // Tops are stored as the matcher bucket "shirt".
            if (value.Contains("shirt") || value.Contains("tee") || value.Contains("top"))
                return "shirt";
            // Bottom garments are stored as the matcher bucket "pants".
            if (value.Contains("pant") || value.Contains("trouser") || value.Contains("jean") || value.Contains("bottom"))
                return "pants";
            // Footwear is stored as the matcher bucket "shoes".
            if (value.Contains("shoe") || value.Contains("sneaker") || value.Contains("boot") || value.Contains("foot"))
                return "shoes";

            // Unknown values are kept instead of discarded, which helps diagnose bad input.
            return value;
        }
    }
}
