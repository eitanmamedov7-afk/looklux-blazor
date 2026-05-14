namespace gadifff.Mobile;

internal static class ServerEndpointProvider
{
    private const string DefaultPort = "7164";
    private const string UrlPreferenceKey = "server_url";

    public static string GetDefaultUrl()
    {
#if ANDROID
        return DeviceInfo.DeviceType == DeviceType.Virtual
            ? "http://10.0.2.2:7164"
            : "http://127.0.0.1:7164";
#elif IOS
        return DeviceInfo.DeviceType == DeviceType.Virtual
            ? "http://localhost:7164"
            : "http://127.0.0.1:7164";
#elif WINDOWS
        return "http://localhost:7164";
#else
        return "http://localhost:7164";
#endif
    }

    public static Uri? GetStartupUri()
    {
        var saved = Preferences.Get(UrlPreferenceKey, string.Empty);
        var savedUri = Sanitize(saved);
        if (savedUri is not null)
            return savedUri;

        return Sanitize(GetDefaultUrl());
    }

    public static IReadOnlyList<Uri> GetStartupCandidates()
    {
        var candidates = new List<Uri>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddCandidate(candidates, seen, Preferences.Get(UrlPreferenceKey, string.Empty));
        AddCandidate(candidates, seen, GetDefaultUrl());

#if ANDROID
        AddCandidate(candidates, seen, $"http://127.0.0.1:{DefaultPort}");
        AddCandidate(candidates, seen, $"http://10.0.2.2:{DefaultPort}");
#elif IOS
        AddCandidate(candidates, seen, $"http://127.0.0.1:{DefaultPort}");
        AddCandidate(candidates, seen, $"http://localhost:{DefaultPort}");
#else
        AddCandidate(candidates, seen, $"http://localhost:{DefaultPort}");
#endif

        return candidates;
    }

    public static bool SaveCustomUrl(string? raw, out Uri? uri)
    {
        uri = Sanitize(raw);
        if (uri is null)
            return false;

        Preferences.Set(UrlPreferenceKey, uri.ToString());
        return true;
    }

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
