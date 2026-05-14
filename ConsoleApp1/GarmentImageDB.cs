// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBL
{
    public class GarmentImageDB : DB
    {
        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
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

            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            try
            {
                return await cmd.ExecuteNonQueryAsync();
            }
            finally
            {
                cmd.Parameters.Clear();
                if (conn.State == System.Data.ConnectionState.Open)
                    await conn.CloseAsync();
            }
        }

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

            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            try
            {
                reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return null;

                var bytes = (byte[])reader["image_bytes"];
                var mime = reader["mime_type"]?.ToString() ?? "application/octet-stream";
                return (bytes, mime);
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    await reader.CloseAsync();

                cmd.Parameters.Clear();

                if (conn.State == System.Data.ConnectionState.Open)
                    await conn.CloseAsync();
            }
        }

        // הסבר: פונקציית שמירה/יצירה. כותבת נתונים חדשים למערכת ומחזירה סטטוס הצלחה/כישלון.
        private void Add(string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
    }
}
