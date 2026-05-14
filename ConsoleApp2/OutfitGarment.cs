// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
using System;

namespace Models
{
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
