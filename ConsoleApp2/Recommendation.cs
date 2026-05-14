// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Recommendation
    {
        public string RecId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string InputMode { get; set; } = string.Empty;   // image | garment_id
        public string InputReference { get; set; } = string.Empty;
        public int TopK { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
