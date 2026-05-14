// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Outfit
    {
        public string OutfitId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public string? ShirtGarmentId { get; set; }
        public string? PantsGarmentId { get; set; }
        public string? ShoesGarmentId { get; set; }

        public int Score { get; set; }
        public int Rank { get; set; } = 1; // 1 = best, 2, 3
        public string? StyleLabel { get; set; }
        public string? Explanation { get; set; }
        public string? RecommendedPlaces { get; set; }
        public string? SeedType { get; set; }
        public string? SeedGarmentId { get; set; }

        // Legacy fields kept for compatibility with existing data
        public bool LabelIsCompatible { get; set; }
        public string LabelSource { get; set; } = string.Empty;  // manual | scraped | heuristic

        // Comma-separated garment ids that initiated the request (for preselect/spin)
        public string? RequestedGarmentIds { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
