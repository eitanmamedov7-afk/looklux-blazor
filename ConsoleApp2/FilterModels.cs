// מה הקובץ עושה: הקובץ מרכז חלק מהמערכת ומשתתף בהפעלת הפרויקט.
// למה הקובץ נדרש: הוא נדרש כדי שהחלק הזה בפרויקט יפעל בצורה ברורה ומסודרת.
// לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר למסכים, לשירותים, למודלים ולשכבת הדיבי לפי השימוש שלו.
// איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים לקבצים שמזמנים את הקוד הזה או לקבצים שהוא מזמן.

// הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
// הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
// לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר לשכבט הדיבי, לשירותים ולדפי התצוגה.
// ייבוא ספריות שמספקות מחלקות, ממשקים ופעולות שהקובץ צריך כדי לעבוד.



// ייבוא ספריות שמספקות מחלקות, ממשקים ופעולות שהקובץ צריך כדי לעבוד.
using System;
using System.Collections.Generic;
using System.Linq;

// הגדרת מרחו שמות שממקם את הקובץ בטבקת הפרויקט המטאימה.
namespace Models
{
    // הגדרת מבנה מרכזי שמרכז נתונים או פעוליות עובר החלק הזה בפרויקט.
    public class GarmentFilterRequest
    {
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Categories { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Subcategories { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Colors { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Seasons { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Materials { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Brands { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Fits { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Patterns { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Tags { get; set; } = new();

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
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

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static List<string> NormalizeList(IEnumerable<string>? values) =>
            (values ?? Array.Empty<string>())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
    }

    // הגדרת מבנה מרכזי שמרכז נתונים או פעוליות עובר החלק הזה בפרויקט.
    public class GarmentFilterOptions
    {
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Categories { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Subcategories { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Colors { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Seasons { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Materials { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Brands { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Fits { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Patterns { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Tags { get; set; } = new();
    }

    // הגדרת מבנה מרכזי שמרכז נתונים או פעוליות עובר החלק הזה בפרויקט.
    public class OutfitFilterRequest
    {
        // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
        public int MinScore { get; set; } = 40;
        // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
        public int MaxScore { get; set; } = 100;

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> SeedTypes { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> StyleLabels { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> GarmentTypes { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Occasions { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Seasons { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> RecommendedPlaces { get; set; } = new();

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public void Normalize()
        {
            MinScore = Math.Clamp(MinScore, 0, 100);
            MaxScore = Math.Clamp(MaxScore, 0, 100);
            // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
            if (MaxScore < MinScore)
                (MinScore, MaxScore) = (MaxScore, MinScore);

            SeedTypes = NormalizeList(SeedTypes);
            StyleLabels = NormalizeList(StyleLabels);
            GarmentTypes = NormalizeList(GarmentTypes);
            Occasions = NormalizeList(Occasions);
            Seasons = NormalizeList(Seasons);
            RecommendedPlaces = NormalizeList(RecommendedPlaces);
        }

        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        private static List<string> NormalizeList(IEnumerable<string>? values) =>
            (values ?? Array.Empty<string>())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
    }

    // הגדרת מבנה מרכזי שמרכז נתונים או פעוליות עובר החלק הזה בפרויקט.
    public class OutfitFilterOptions
    {
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> SeedTypes { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> StyleLabels { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> GarmentTypes { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Occasions { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> Seasons { get; set; } = new();
        // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
        public List<string> RecommendedPlaces { get; set; } = new();
    }
}
