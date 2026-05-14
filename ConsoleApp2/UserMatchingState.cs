// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
using System;

namespace Models
{
    public class UserMatchingState
    {
        public string UserId { get; set; } = string.Empty;

        // Minimum garments per type required before matching is allowed (5 by default, 6 after failure)
        public int MinPerType { get; set; } = 5;

        // Set to true when a failure occurred and user must reach MinPerType across all types
        public bool LockedAfterFailure { get; set; }

        public DateTime? LastFailureAt { get; set; }
        public DateTime? LastSuccessAt { get; set; }
    }
}
