
// SEARCH INDEX
// MODEL, GARMENT, CLOSET, IMAGE, UPLOAD, FILTER, MATCH, AI, SHA256
//
// Topic: GARMENT MODEL
// Purpose: Represents one closet item, including image data and AI-extracted features.
// Search keywords: MODEL GARMENT CLOSET IMAGE UPLOAD FILTER MATCH AI SHA256
// When to use it: Show this when explaining what data a garment stores after upload.
// Important notes: Current schema stores one image directly on the garment row.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace Models
{
    // SECTION: GARMENT DATA SHAPE
    // Topic: Garment data model
    // Purpose: Holds closet item fields used by upload, filtering, image display, and matching.
    // Search keywords: MODEL GARMENT IMAGE FEATURE FILTER MATCH
    // When to use it: Use when tracing a garment from DB to closet card/API response.
    // Important notes: Type should normalize into shirt, pants, or shoes for matching.
    public class Garment
    {
        public string GarmentId { get; set; } = string.Empty;

        public string OwnerUserId { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string? ColorSecondary { get; set; }
        public string? Pattern { get; set; }
        public string? StyleCategory { get; set; }
        public string? Season { get; set; }
        public string? Occasion { get; set; }
        public int? FormalityLevel { get; set; }
        public string? StyleTags { get; set; }
        public string? Fit { get; set; }
        public string? Material { get; set; }
        public string? Sleeve { get; set; }
        public string? Length { get; set; }
        public string? Brand { get; set; }

        public string? FeatureJson { get; set; }

        [JsonIgnore]
        public byte[]? ImageBytes { get; set; }

        public string ImageMimeType { get; set; } = "image/jpeg";

        public string Sha256 { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
