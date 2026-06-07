// SEARCH INDEX
// OUTFIT_WEAR_LOG, DATABASE, OUTFIT, HISTORY, TIMESTAMP, COUNT
//
// Topic: Outfit wear log model
// Purpose: Represents one time a saved outfit was marked as worn, and can also carry count/first/last summary values.
// Search keywords: OUTFIT_WEAR_LOG DATABASE OUTFIT HISTORY TIMESTAMP COUNT
// When to use it: Show this when explaining the SQL table or the per-outfit wear summary.
// Important notes: This is outfit-only. It does not log individual garments. Summary fields are calculated, not extra SQL columns.

namespace Models;

public class OutfitWearLog
{
    // FLOW_OUTFIT_WEAR_DATA_01: OutfitWearLog is the data shape for one "I wore this outfit" record.
    // This file is involved because DB/API/UI all pass this same simple model; next step is OutfitWearLogDB.
    // FLOW_OUTFIT_WEAR_STATS_03: The same model also carries aggregated wear count, first worn, and last worn values.
    // This keeps the project with one outfit-wear model instead of a separate summary-only model.
    public string WearLogId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string OutfitId { get; set; } = string.Empty;
    public DateTime WornAt { get; set; } = DateTime.UtcNow;
    public int WearCount { get; set; }
    public DateTime? FirstWornAt { get; set; }
    public DateTime? LastWornAt { get; set; }
}
