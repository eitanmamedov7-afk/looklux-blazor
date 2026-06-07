namespace DBL
{
    // Shared normalization for the three garment buckets used by the closet, outfits, and outfit_garments tables.
    internal static class GarmentTypeNormalizer
    {
        public static string Normalize(string? raw)
        {
            var value = (raw ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            if (value.Contains("shirt") || value.Contains("t-shirt") || value.Contains("tee") || value.Contains("top"))
                return "shirt";

            if (value.Contains("pant") || value.Contains("trouser") || value.Contains("jean") || value.Contains("bottom"))
                return "pants";

            if (value.Contains("shoe") || value.Contains("sneaker") || value.Contains("boot") || value.Contains("foot"))
                return "shoes";

            return value;
        }

        public static string BuildSqlCaseExpression(string sqlValueExpression)
        {
            return $@"(CASE
                        WHEN {sqlValueExpression} LIKE '%shirt%' OR {sqlValueExpression} LIKE '%t-shirt%' OR {sqlValueExpression} LIKE '%tee%' OR {sqlValueExpression} LIKE '%top%' THEN 'shirt'
                        WHEN {sqlValueExpression} LIKE '%pant%' OR {sqlValueExpression} LIKE '%trouser%' OR {sqlValueExpression} LIKE '%jean%' OR {sqlValueExpression} LIKE '%bottom%' THEN 'pants'
                        WHEN {sqlValueExpression} LIKE '%shoe%' OR {sqlValueExpression} LIKE '%sneaker%' OR {sqlValueExpression} LIKE '%boot%' OR {sqlValueExpression} LIKE '%foot%' THEN 'shoes'
                        ELSE {sqlValueExpression}
                      END)";
        }
    }
}
