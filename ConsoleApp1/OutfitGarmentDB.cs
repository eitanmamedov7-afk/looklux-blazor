
// SEARCH INDEX
// DATABASE, OUTFIT, GARMENT, LINK, RELATION, ADD, REMOVE, LIST, SEARCH, ROLE
//
// Topic: OUTFIT GARMENT LINK DATABASE
// Purpose: Manages the link rows that connect one saved outfit to its shirt, pants, and shoes.
// Search keywords: DATABASE OUTFIT GARMENT LINK RELATION ADD REMOVE LIST SEARCH ROLE
// When to use it: Show this when explaining why saved outfits can display three garment images.
// Important notes: Current table columns are outfit_id, garment_id, and role.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;

namespace DBL
{
    // SECTION: OUTFIT_GARMENTS LINK TABLE
    // Topic: OutfitGarmentDB class
    // Purpose: Table-specific DB logic for eitan_project12.outfit_garments.
    // Search keywords: DATABASE LINK OUTFIT GARMENT ADD REMOVE LIST
    // When to use it: Use after saving an outfit or before deleting an outfit.
    // Important notes: This is a relation table, not a standalone user-facing object.
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
                GarmentType = GarmentTypeNormalizer.Normalize(row[2]?.ToString()),
                IsSeed = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Inserts one link row between an outfit and one garment.
        // The role value is normalized so matching/filtering always sees shirt/pants/shoes.
        // FLOW_MATCH_SAVE_WEB_05 / FLOW_MATCH_SAVE_MOBILE_06:
        // OutfitGarmentDB.CreateAsync inserts one outfit-to-garment link row for a role.
        // This DB file is involved because outfits are connected to garments through outfit_garments; next step is saved response.
        public Task<int> CreateAsync(OutfitGarment item)
        {
            // Field names match the current outfit_garments table columns.
            var fields = new Dictionary<string, object>
            {
                ["outfit_id"] = item.OutfitId,
                ["garment_id"] = item.GarmentId,
                ["role"] = GarmentTypeNormalizer.Normalize(item.GarmentType)
            };

            // BaseDB builds and executes the INSERT.
            return InsertAsync(fields);
        }

        // Saves all garment links for one outfit.
        // MatchingService passes the three rows after it creates an outfit recommendation.
        // FLOW_MATCH_SAVE_WEB_05A / FLOW_MATCH_SAVE_MOBILE_06A:
        // OutfitGarmentDB.CreateManyAsync writes the shirt/pants/shoes relation rows after outfit insert.
        // This DB file is involved because saved outfit display needs these links; final step is UI/API success.
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
        // FLOW_OUTFIT_VIEW_WEB_04:
        // OutfitGarmentDB.GetByOutfitIdsAsync loads linked garment rows for saved outfit display.
        // This DB file is involved because outfit cards need their garment pieces; next step is Outfits.razor rendering.
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
        // FLOW_DELETE_OUTFIT_WEB_03 / FLOW_DELETE_OUTFIT_MOBILE_04 / FLOW_DELETE_ACCOUNT_WEB_03 / FLOW_DELETE_ACCOUNT_MOBILE_04:
        // OutfitGarmentDB.DeleteByOutfitIdAsync removes child links before deleting outfit rows.
        // This DB file is involved because outfit_garments depends on outfits; next step is OutfitDB.DeleteOutfitAsync.
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

    }
}
