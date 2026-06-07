
// SEARCH INDEX
// MODEL, OUTFIT, RECOMMENDATION, MATCH, SAVE, SCORE, FILTER, GARMENT
//
// Topic: OUTFIT MODEL
// Purpose: Represents one saved recommendation/outfit result.
// Search keywords: MODEL OUTFIT RECOMMENDATION MATCH SAVE SCORE FILTER GARMENT
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
    }
}
