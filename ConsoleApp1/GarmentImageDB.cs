// מה הקובץ עושה: הקובץ מרכז חלק מהמערכת ומשתתף בהפעלת הפרויקט.
// למה הקובץ נדרש: הוא נדרש כדי שהחלק הזה בפרויקט יפעל בצורה ברורה ומסודרת.
// לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר למסכים, לשירותים, למודלים ולשכבת הדיבי לפי השימוש שלו.
// איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים לקבצים שמזמנים את הקוד הזה או לקבצים שהוא מזמן.

// מה הקובץ עושה: הקובץ מטפל בגישה למסד הנתונים ובתרגום נתונים לשכבט הקוד.
// הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
// לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר למודלם, לשירותים, לדפי הניהול, לדף הארון ולשירות ההתאמות.
// איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים במודלם שמוחזרים מכאן ובדפים או בשירותים שקוראים לפעוליות הדיבי.



// ייבוא ספריות שמספקות מחלקות, ממשקים ופעולות שהקובץ צריך כדי לעבוד.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// הגדרת מרחו שמות שממקם את הקובץ בטבקת הפרויקט המטאימה.
namespace DBL
{
    // הגדרת מבנה מרכזי שמרכז נתונים או פעוליות עובר החלק הזה בפרויקט.
    public class GarmentImageDB : DB
    {
        public async Task<int> CreateAsync(
            string imageId,
            string garmentId,
            string userId,
            string mimeType,
            string fileName,
            byte[] imageBytes,
            string sha256)
        {
            const string sql = @"
INSERT INTO eitan_project12.garment_images
(image_id, garment_id, user_id, mime_type, file_name, image_bytes, sha256)
VALUES
(@image_id, @garment_id, @user_id, @mime_type, @file_name, @image_bytes, @sha256);";

            cmd.CommandText = sql;
            cmd.Parameters.Clear();

            Add("@image_id", imageId);
            Add("@garment_id", garmentId);
            Add("@user_id", userId);
            Add("@mime_type", mimeType);
            Add("@file_name", string.IsNullOrWhiteSpace(fileName) ? (object)DBNull.Value : fileName);
            Add("@image_bytes", imageBytes);
            Add("@sha256", sha256);

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (conn.State != System.Data.ConnectionState.Open)
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                await conn.OpenAsync();

            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                return await cmd.ExecuteNonQueryAsync();
            }
            // ניקוי או סגירת משאבים שסרוצים בסוף הפעולה.
            finally
            {
                cmd.Parameters.Clear();
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (conn.State == System.Data.ConnectionState.Open)
                    // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                    await conn.CloseAsync();
            }
        }

        // הגדרת פעולה אסינכרונית שמובצעת מול שירות, דיבי או תצצוגה בלי לחסום את ההרצה.
        public async Task<(byte[] Bytes, string MimeType)?> GetLatestByGarmentIdAsync(string garmentId)
        {
            const string sql = @"
SELECT image_bytes, mime_type
FROM eitan_project12.garment_images
WHERE garment_id = @gid
ORDER BY created_at DESC
LIMIT 1;";

            cmd.CommandText = sql;
            cmd.Parameters.Clear();

            Add("@gid", garmentId);

            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (conn.State != System.Data.ConnectionState.Open)
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                await conn.OpenAsync();

            // ניסיון להריץ פעולה שעלולה להיכשל, כדי שאפשר יהיה לטפל בכשל בצורה מסודרת.
            try
            {
                // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                reader = await cmd.ExecuteReaderAsync();
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (!await reader.ReadAsync())
                    // החזרת התוצאה אל הקוד שקרא לפעולה.
                    return null;

                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var bytes = (byte[])reader["image_bytes"];
                // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
                var mime = reader["mime_type"]?.ToString() ?? "application/octet-stream";
                // החזרת התוצאה אל הקוד שקרא לפעולה.
                return (bytes, mime);
            }
            // ניקוי או סגירת משאבים שסרוצים בסוף הפעולה.
            finally
            {
                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (reader != null && !reader.IsClosed)
                    // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                    await reader.CloseAsync();

                cmd.Parameters.Clear();

                // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
                if (conn.State == System.Data.ConnectionState.Open)
                    // המתנה לפעולה אסינכרונית כדי להמשיך רק אחרי שהפעולה הסתיימה.
                    await conn.CloseAsync();
            }
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private void Add(string name, object value)
        {
            // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
    }
}
