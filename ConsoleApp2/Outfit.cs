
// SEARCH INDEX
// MODEL, OUTFIT, RECOMMENDATION, MATCH, SAVE, SCORE, FILTER, GARMENT, OUTFIT_WEAR_LOG
//
// Topic: OUTFIT MODEL
// Purpose: Represents one saved recommendation/outfit result.
// Search keywords: MODEL OUTFIT RECOMMENDATION MATCH SAVE SCORE FILTER GARMENT OUTFIT_WEAR_LOG
// When to use it: Show this when explaining saved outfits and recommendation results.
// Important notes: Shirt/Pants/Shoes ids point to garments; outfit_garments also stores relation rows.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    // SECTION: OUTFIT DATA SHAPE
    // Topic: Outfit data model
    // Purpose: Holds recommendation metadata, score, explanation, seed, and linked garment ids.
    // Search keywords: MODEL OUTFIT SCORE RECOMMENDATION GARMENT
    // When to use it: Use when tracing a saved recommendation from MatchingService to Outfits page.
    // Important notes: This is not the closet item; it is the saved combination result.
    public class Outfit
    {
        public string OutfitId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public string? ShirtGarmentId { get; set; }
        public string? PantsGarmentId { get; set; }
        public string? ShoesGarmentId { get; set; }

        public int Score { get; set; }
        public int Rank { get; set; } = 1;
        public string? StyleLabel { get; set; }
        public string? Explanation { get; set; }
        public string? RecommendedPlaces { get; set; }
        public string? SeedType { get; set; }
        public string? SeedGarmentId { get; set; }

        public bool LabelIsCompatible { get; set; }
        public string LabelSource { get; set; } = string.Empty;

        public string? RequestedGarmentIds { get; set; }

        public DateTime CreatedAt { get; set; }

        // Topic: Outfit wear summary fields
        // Purpose: Carries how many times this saved outfit was worn and the first/last worn timestamps.
        // Search keywords: MODEL OUTFIT_WEAR_LOG OUTFIT HISTORY COUNT TIMESTAMP
        // When to use it: Use when rendering outfit cards in the web page or MAUI app.
        // Important notes: These values come from outfit_wear_logs and are not stored in the outfits table.
        // FLOW_OUTFIT_WEAR_STATS_04: Outfit model carries wear count, first worn, and last worn to UI/API cards.
        // This file is involved because web and MAUI both display Outfit objects; next step is card rendering.
        public int WearCount { get; set; }
        public DateTime? FirstWornAt { get; set; }
        public DateTime? LastWornAt { get; set; }
    }
}
