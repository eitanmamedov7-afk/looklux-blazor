// זרימת קובץ: הקובץ מטפל בקלט, מבצע עיבוד לפי כללי המערכת, ומחזיר תוצאה/עדכון מצב באופן עקבי.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Models
{
    public class GarmentFilterRequest
    {
        public List<string> Categories { get; set; } = new();
        public List<string> Subcategories { get; set; } = new();
        public List<string> Colors { get; set; } = new();
        public List<string> Seasons { get; set; } = new();
        public List<string> Materials { get; set; } = new();
        public List<string> Brands { get; set; } = new();
        public List<string> Fits { get; set; } = new();
        public List<string> Patterns { get; set; } = new();
        public List<string> Tags { get; set; } = new();

        // הסבר: פונקציית נרמול. מנקה ומאחד פורמט נתונים כדי למנוע חוסר עקביות בהמשך הזרימה.
        public void Normalize()
        {
            Categories = NormalizeList(Categories);
            Subcategories = NormalizeList(Subcategories);
            Colors = NormalizeList(Colors);
            Seasons = NormalizeList(Seasons);
            Materials = NormalizeList(Materials);
            Brands = NormalizeList(Brands);
            Fits = NormalizeList(Fits);
            Patterns = NormalizeList(Patterns);
            Tags = NormalizeList(Tags);
        }

        // הסבר: פונקציית נרמול. מנקה ומאחד פורמט נתונים כדי למנוע חוסר עקביות בהמשך הזרימה.
        private static List<string> NormalizeList(IEnumerable<string>? values) =>
            (values ?? Array.Empty<string>())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
    }

    public class GarmentFilterOptions
    {
        public List<string> Categories { get; set; } = new();
        public List<string> Subcategories { get; set; } = new();
        public List<string> Colors { get; set; } = new();
        public List<string> Seasons { get; set; } = new();
        public List<string> Materials { get; set; } = new();
        public List<string> Brands { get; set; } = new();
        public List<string> Fits { get; set; } = new();
        public List<string> Patterns { get; set; } = new();
        public List<string> Tags { get; set; } = new();
    }

    public class OutfitFilterRequest
    {
        public int MinScore { get; set; } = 40;
        public int MaxScore { get; set; } = 100;

        public List<string> SeedTypes { get; set; } = new();
        public List<string> StyleLabels { get; set; } = new();
        public List<string> GarmentTypes { get; set; } = new();
        public List<string> Occasions { get; set; } = new();
        public List<string> Seasons { get; set; } = new();
        public List<string> RecommendedPlaces { get; set; } = new();

        // הסבר: פונקציית נרמול. מנקה ומאחד פורמט נתונים כדי למנוע חוסר עקביות בהמשך הזרימה.
        public void Normalize()
        {
            MinScore = Math.Clamp(MinScore, 0, 100);
            MaxScore = Math.Clamp(MaxScore, 0, 100);
            if (MaxScore < MinScore)
                (MinScore, MaxScore) = (MaxScore, MinScore);

            SeedTypes = NormalizeList(SeedTypes);
            StyleLabels = NormalizeList(StyleLabels);
            GarmentTypes = NormalizeList(GarmentTypes);
            Occasions = NormalizeList(Occasions);
            Seasons = NormalizeList(Seasons);
            RecommendedPlaces = NormalizeList(RecommendedPlaces);
        }

        // הסבר: פונקציית נרמול. מנקה ומאחד פורמט נתונים כדי למנוע חוסר עקביות בהמשך הזרימה.
        private static List<string> NormalizeList(IEnumerable<string>? values) =>
            (values ?? Array.Empty<string>())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
    }

    public class OutfitFilterOptions
    {
        public List<string> SeedTypes { get; set; } = new();
        public List<string> StyleLabels { get; set; } = new();
        public List<string> GarmentTypes { get; set; } = new();
        public List<string> Occasions { get; set; } = new();
        public List<string> Seasons { get; set; } = new();
        public List<string> RecommendedPlaces { get; set; } = new();
    }
}
