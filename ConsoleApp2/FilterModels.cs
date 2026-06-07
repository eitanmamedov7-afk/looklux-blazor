



using System;
using System.Collections.Generic;
using System.Linq;

namespace Models
{
    public class GarmentFilterRequest
    {
        public List<string> Categories { get; set; } = new();
        public List<string> Colors { get; set; } = new();
        public List<string> Seasons { get; set; } = new();
        public List<string> Occasions { get; set; } = new();
        public List<string> Brands { get; set; } = new();

        public void Normalize()
        {
            Categories = NormalizeList(Categories);
            Colors = NormalizeList(Colors);
            Seasons = NormalizeList(Seasons);
            Occasions = NormalizeList(Occasions);
            Brands = NormalizeList(Brands);
        }

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
        public List<string> Colors { get; set; } = new();
        public List<string> Seasons { get; set; } = new();
        public List<string> Occasions { get; set; } = new();
        public List<string> Brands { get; set; } = new();
    }

    public class OutfitFilterRequest
    {
        public int MinScore { get; set; } = 40;
        public int MaxScore { get; set; } = 100;

        public List<string> StyleLabels { get; set; } = new();
        public List<string> Seasons { get; set; } = new();
        public List<string> RecommendedPlaces { get; set; } = new();

        public void Normalize()
        {
            MinScore = Math.Clamp(MinScore, 0, 100);
            MaxScore = Math.Clamp(MaxScore, 0, 100);
            if (MaxScore < MinScore)
                (MinScore, MaxScore) = (MaxScore, MinScore);

            StyleLabels = NormalizeList(StyleLabels);
            Seasons = NormalizeList(Seasons);
            RecommendedPlaces = NormalizeList(RecommendedPlaces);
        }

        private static List<string> NormalizeList(IEnumerable<string>? values) =>
            (values ?? Array.Empty<string>())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
    }

    public class OutfitFilterOptions
    {
        public List<string> StyleLabels { get; set; } = new();
        public List<string> Seasons { get; set; } = new();
        public List<string> RecommendedPlaces { get; set; } = new();
    }
}
