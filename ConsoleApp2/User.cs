// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class User
    {
        public string UserId { get; set; } = "";       // char(36)
        public string Email { get; set; } = "";        // varchar(255)
        public string FullName { get; set; } = "";     // varchar(120)
        public string Role { get; set; } = "customer"; // admin|stylist|customer
        public string PasswordHash { get; set; } = ""; // varchar(255)
        public DateTime CreatedAt { get; set; }        // timestamp
    }
}
