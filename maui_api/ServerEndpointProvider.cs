// הקובץ מרכז את ניהול כתובת ה-API באפליקציית המובייל.
// הוא בוחר כתובת ברירת מחדל לפי פלטפורמה, טוען כתובת שמורה,
// מנקה ומאמת קלט מהמשתמש, ובונה רשימת כתובות אפשריות להתחברות.

namespace gadifff.Mobile;

internal static class ServerEndpointProvider
{
    // פורט ברירת מחדל ל-API הנוכחי, ופורט ישן לתאימות לאחור.
    private const string DefaultPort = "7166";
    private const string LegacyPort = "7164";

    // מפתח שבו נשמרת כתובת שרת מותאמת אישית בהעדפות המכשיר.
    private const string UrlPreferenceKey = "server_url";

    // מחזירה כתובת API ברירת מחדל לפי פלטפורמה וסוג מכשיר (אמולטור/פיזי).
    public static string GetDefaultUrl()
    {
#if ANDROID
        // באנדרואיד אמולטור מגיעים למחשב המארח דרך 10.0.2.2.
        return DeviceInfo.DeviceType == DeviceType.Virtual
            ? $"http://10.0.2.2:{DefaultPort}"
            : $"http://127.0.0.1:{DefaultPort}";
#elif IOS
        // ב-iOS אמולטור משתמשים ב-localhost, ובמכשיר פיזי חוזרים ל-127.0.0.1.
        return DeviceInfo.DeviceType == DeviceType.Virtual
            ? $"http://localhost:{DefaultPort}"
            : $"http://127.0.0.1:{DefaultPort}";
#elif WINDOWS
        return $"http://localhost:{DefaultPort}";
#else
        return $"http://localhost:{DefaultPort}";
#endif
    }

    // מחזירה את כתובת ההפעלה הראשית: קודם כתובת שמורה ותקינה, אחרת ברירת מחדל.
    public static Uri? GetStartupUri()
    {
        var saved = Preferences.Get(UrlPreferenceKey, string.Empty);
        var savedUri = Sanitize(saved);
        if (savedUri is not null)
            return savedUri;

        return Sanitize(GetDefaultUrl());
    }

    // בונה רשימת כתובות מועמדות לפי סדר עדיפויות לחיבור ראשוני לשרת.
    // הרשימה כוללת כתובת שמורה, ברירת מחדל, וכתובות תאימות לפורט הישן.
    public static IReadOnlyList<Uri> GetStartupCandidates()
    {
        var candidates = new List<Uri>();
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

        return candidates;
    }

    // שומרת כתובת שרת מותאמת אישית רק אם היא תקינה.
    // מחזירה true אם נשמר בהצלחה, ו-false אם הקלט לא חוקי.
    public static bool SaveCustomUrl(string? raw, out Uri? uri)
    {
        uri = Sanitize(raw);
        if (uri is null)
            return false;

        Preferences.Set(UrlPreferenceKey, uri.ToString());
        return true;
    }

    // מנקה ומאמת כתובת שרת:
    // 1) מסירה רווחים, 2) מוסיפה http:// אם חסר פרוטוקול,
    // 3) בודקת URI מוחלט, 4) מאשרת רק http/https.
    public static Uri? Sanitize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var value = raw.Trim();
        if (!value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            value = "http://" + value;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return null;

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return uri;
    }

    // מוסיפה כתובת לרשימת המועמדות רק אם היא תקינה ועדיין לא נוספה קודם.
    private static void AddCandidate(ICollection<Uri> candidates, ISet<string> seen, string? raw)
    {
        var uri = Sanitize(raw);
        if (uri is null)
            return;

        var key = uri.ToString();
        if (!seen.Add(key))
            return;

        candidates.Add(uri);
    }
}
