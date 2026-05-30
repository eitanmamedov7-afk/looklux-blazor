// מה הקובץ עושה: הקובץ מרכז חלק מהמערכת ומשתתף בהפעלת הפרויקט.
// למה הקובץ נדרש: הוא נדרש כדי שהחלק הזה בפרויקט יפעל בצורה ברורה ומסודרת.
// לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר למסכים, לשירותים, למודלים ולשכבת הדיבי לפי השימוש שלו.
// איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים לקבצים שמזמנים את הקוד הזה או לקבצים שהוא מזמן.

// מה הקובץ עושה: הקובץ שיך לאפליקציית המובייל ומגדיר מסך, אתחול או התאמה לפלטפורמה.
// הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
// לאילו חלקים בפרויקט הוא מתחבר: הוא מתחבר לפרויקט האטר, להגדרי המובייל, לדף הראוי ולכטובת השרת.
// איפה ממשיכים לקרוא את הלוגיקה הקשורה: ממשיכים בדף הראוי של המובייל ובקוד שמוחליט לאיזה כטובת שרת להתחבר.



// הגדרת מרחו שמות שממקם את הקובץ בטבקת הפרויקט המטאימה.
namespace gadifff.Mobile;

// הגדרת מבנה מרכזי שמרכז נתונים או פעוליות עובר החלק הזה בפרויקט.
internal static class ServerEndpointProvider
{
    // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
    private const string DefaultPort = "7166";
    private const string LegacyPort = "7164";
    // הגדרת משתנה או שדה ששומר מצב, ערך או תלות שנדרשים להמשך הקוד.
    private const string UrlPreferenceKey = "server_url";

    // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
    public static string GetDefaultUrl()
    {
#if ANDROID
        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return DeviceInfo.DeviceType == DeviceType.Virtual
            ? $"http://10.0.2.2:{DefaultPort}"
            : $"http://127.0.0.1:{DefaultPort}";
#elif IOS
        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return DeviceInfo.DeviceType == DeviceType.Virtual
            ? $"http://localhost:{DefaultPort}"
            : $"http://127.0.0.1:{DefaultPort}";
#elif WINDOWS
        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return $"http://localhost:{DefaultPort}";
#else
        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return $"http://localhost:{DefaultPort}";
#endif
    }

    // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
    public static Uri? GetStartupUri()
    {
        // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
        var saved = Preferences.Get(UrlPreferenceKey, string.Empty);
        // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
        var savedUri = Sanitize(saved);
        // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if (savedUri is not null)
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return savedUri;

        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return Sanitize(GetDefaultUrl());
    }

    // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
    public static IReadOnlyList<Uri> GetStartupCandidates()
    {
        // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
        var candidates = new List<Uri>();
        // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddCandidate(candidates, seen, Preferences.Get(UrlPreferenceKey, string.Empty));
        AddCandidate(candidates, seen, GetDefaultUrl());

#if ANDROID
        AddCandidate(candidates, seen, $"http://127.0.0.1:{DefaultPort}");
        AddCandidate(candidates, seen, $"http://10.0.2.2:{DefaultPort}");
        AddCandidate(candidates, seen, $"http://127.0.0.1:{LegacyPort}");
        AddCandidate(candidates, seen, $"http://10.0.2.2:{LegacyPort}");
#elif IOS
        AddCandidate(candidates, seen, $"http://127.0.0.1:{DefaultPort}");
        AddCandidate(candidates, seen, $"http://localhost:{DefaultPort}");
        AddCandidate(candidates, seen, $"http://127.0.0.1:{LegacyPort}");
        AddCandidate(candidates, seen, $"http://localhost:{LegacyPort}");
#else
        AddCandidate(candidates, seen, $"http://localhost:{DefaultPort}");
        AddCandidate(candidates, seen, $"http://localhost:{LegacyPort}");
#endif

        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return candidates;
    }

    // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
    public static bool SaveCustomUrl(string? raw, out Uri? uri)
    {
        uri = Sanitize(raw);
        // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if (uri is null)
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return false;

        Preferences.Set(UrlPreferenceKey, uri.ToString());
        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return true;
    }

    // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
    public static Uri? Sanitize(string? raw)
    {
        // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if (string.IsNullOrWhiteSpace(raw))
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return null;

        // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
        var value = raw.Trim();
        // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if (!value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            value = "http://" + value;
        }

        // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return null;

        // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return null;
        }

        // החזרת התוצאה אל הקוד שקרא לפעולה.
        return uri;
    }

    // הגדרת פעולה שמרכזת שלב ברור בלוגיקה ומופעלת כאשר המסך או השירות צריך את התוצאה שלה.
    private static void AddCandidate(ICollection<Uri> candidates, ISet<string> seen, string? raw)
    {
        // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
        var uri = Sanitize(raw);
        // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if (uri is null)
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return;

        // יצירת משתנה מקומי שמכין ערך ביניים להמשך הפעולה.
        var key = uri.ToString();
        // בדיקת תנאי שמוחליטה האם להמשיך, לעהצור או לעבור למסלול אחר.
        if (!seen.Add(key))
            // החזרת התוצאה אל הקוד שקרא לפעולה.
            return;

        candidates.Add(uri);
    }
}
