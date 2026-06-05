



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBL
{
    // Data-access class for garment photos.
    // This affects the closet upload process and every place in the UI that displays a garment image.
    // The web project compiles this shared DB layer directly and registers GarmentImageDB in DI.
    // Closet.razor uses it after a garment upload, and Program.cs uses it to serve image bytes
    // through /media/garments/by-garment/{garmentId}.
    public class GarmentImageDB : DB
    {
        // Saves the single uploaded image row during the closet garment-upload process.
        // The database should enforce one image per garment with a UNIQUE key on garment_id.
        // If another image already exists for the same garment_id, this insert is expected to fail.
        // The image is stored as raw bytes in the garment_images table, together with metadata
        // needed later to serve it correctly: MIME type, original filename, owner user id, and hash.
        public async Task<int> CreateAsync(
            string imageId,
            string garmentId,
            string userId,
            string mimeType,
            string fileName,
            byte[] imageBytes,
            string sha256)
        {
            // Parameterized INSERT keeps the binary data and user-provided file name out of the SQL text.
            const string sql = @"
INSERT INTO eitan_project12.garment_images
(image_id, garment_id, user_id, mime_type, file_name, image_bytes, sha256)
VALUES
(@image_id, @garment_id, @user_id, @mime_type, @file_name, @image_bytes, @sha256);";

            cmd.CommandText = sql;
            cmd.Parameters.Clear();

            // Match each method argument to the real garment_images table columns.
            Add("@image_id", imageId);
            Add("@garment_id", garmentId);
            Add("@user_id", userId);
            Add("@mime_type", mimeType);
            Add("@file_name", string.IsNullOrWhiteSpace(fileName) ? (object)DBNull.Value : fileName);
            Add("@image_bytes", imageBytes);
            Add("@sha256", sha256);

            // Open the shared connection only when this operation needs it.
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            try
            {
                // Returns the number of inserted rows. The upload flow treats <= 0 as failure.
                return await cmd.ExecuteNonQueryAsync();
            }
            finally
            {
                // Clear parameters and close the connection so the reused command/connection
                // starts clean for the next DB operation.
                cmd.Parameters.Clear();
                if (conn.State == System.Data.ConnectionState.Open)
                    await conn.CloseAsync();
            }
        }

        // Gets the stored image for a garment when the browser requests the garment image URL.
        // The UI and media endpoint only need the binary bytes plus MIME type to return an HTTP file.
        public async Task<(byte[] Bytes, string MimeType)?> GetByGarmentIdAsync(string garmentId)
        {
            // garment_id is unique in garment_images, so this query should return zero or one row.
            const string sql = @"
SELECT image_bytes, mime_type
FROM eitan_project12.garment_images
WHERE garment_id = @gid;";

            cmd.CommandText = sql;
            cmd.Parameters.Clear();

            // The caller gives a garment id, not an image id, because each garment has only one image.
            Add("@gid", garmentId);

            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            try
            {
                reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return null;

                // image_bytes is the actual BLOB content; mime_type tells the browser how to display it.
                var bytes = (byte[])reader["image_bytes"];
                var mime = reader["mime_type"]?.ToString() ?? "application/octet-stream";
                return (bytes, mime);
            }
            finally
            {
                // Readers must be closed before reusing the command/connection for another query.
                if (reader != null && !reader.IsClosed)
                    await reader.CloseAsync();

                cmd.Parameters.Clear();

                if (conn.State == System.Data.ConnectionState.Open)
                    await conn.CloseAsync();
            }
        }

        // Small helper for adding command parameters consistently.
        // Null values are converted to DBNull because database providers do not accept C# null directly.
        private void Add(string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
    }
}
