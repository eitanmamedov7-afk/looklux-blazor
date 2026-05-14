// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Models
{
    public class Garment
    {
        public string GarmentId { get; set; } = string.Empty;

        // FK -> users.user_id
        public string OwnerUserId { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;      // shirt | pants | shoes
        public string? Color { get; set; }
        public string? ColorSecondary { get; set; }
        public string? Pattern { get; set; }
        public string? StyleCategory { get; set; }
        public string? Season { get; set; }
        public string? Occasion { get; set; }
        public int? FormalityLevel { get; set; }
        // JSON array string, e.g. ["minimal","street"]
        public string? StyleTags { get; set; }
        public string? Fit { get; set; }
        public string? Material { get; set; }
        public string? Sleeve { get; set; }
        public string? Length { get; set; }
        public string? Brand { get; set; }

        // נשאר אופציונלי, אבל אנחנו נציג תמונה דרך garment_images
        public string? ImageUrl { get; set; }

        public string? FeatureJson { get; set; }

        // dedupe per user
        public string GarmentHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
