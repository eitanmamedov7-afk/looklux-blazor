
// SEARCH INDEX
// MODEL, OUTFIT, GARMENT, LINK, RELATION, ROLE, MATCH
//
// Topic: OUTFIT GARMENT LINK MODEL
// Purpose: Represents one relation between a saved outfit and one garment.
// Search keywords: MODEL OUTFIT GARMENT LINK RELATION ROLE MATCH
// When to use it: Show this when explaining how one outfit connects to shirt/pants/shoes.
// Important notes: Three rows normally belong to one saved outfit.

using System;

namespace Models
{
    // SECTION: OUTFIT GARMENT LINK DATA SHAPE
    // Topic: OutfitGarment data model
    // Purpose: Holds relation metadata for outfit_id + garment_id + role.
    // Search keywords: MODEL LINK OUTFIT GARMENT ROLE
    // When to use it: Use when tracing MatchingService save into outfit_garments table.
    // Important notes: Role should normalize to shirt, pants, or shoes.
    public class OutfitGarment
    {
        public string OutfitGarmentId { get; set; } = string.Empty;
        public string OutfitId { get; set; } = string.Empty;
        public string GarmentId { get; set; } = string.Empty;
        public string GarmentType { get; set; } = string.Empty;
        public bool IsSeed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
