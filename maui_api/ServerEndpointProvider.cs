namespace gadifff.Mobile;

internal static class ServerEndpointProvider
{
    private const string DefaultPort = "7166";
    private const string LegacyPort = "7164";
    private const string UrlPreferenceKey = "server_url";

    public static string GetDefaultUrl()
    {
#if ANDROID
        return DeviceInfo.DeviceType == DeviceType.Virtual
            ? $"http://10.0.2.2:{DefaultPort}"
            : $"http://127.0.0.1:{DefaultPort}";
#elif IOS
        return DeviceInfo.DeviceType == DeviceType.Virtual
            ? $"http://localhost:{DefaultPort}"
            : $"http://127.0.0.1:{DefaultPort}";
#else
        return $"http://localhost:{DefaultPort}";
#endif
    }

    public static IReadOnlyList<Uri> GetStartupCandidates()
    {
        var candidates = new List<Uri>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var saved = Preferences.Get(UrlPreferenceKey, string.Empty);
        var savedUri = Sanitize(saved);
        if (savedUri is not null)
        {
            Preferences.Set(UrlPreferenceKey, savedUri.ToString());
            AddCandidate(candidates, seen, savedUri.ToString());
        }

        AddCandidate(candidates, seen, GetDefaultUrl());

#if ANDROID
        AddCandidate(candidates, seen, $"http://10.0.2.2:{DefaultPort}");
        AddCandidate(candidates, seen, $"http://127.0.0.1:{DefaultPort}");
#elif IOS
        AddCandidate(candidates, seen, $"http://localhost:{DefaultPort}");
        AddCandidate(candidates, seen, $"http://127.0.0.1:{DefaultPort}");
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

        if (uri.Port.ToString() == LegacyPort)
        {
            var builder = new UriBuilder(uri)
            {
                Port = int.Parse(DefaultPort)
            };
            return builder.Uri;
        }

        return uri;
    }

    private static void AddCandidate(ICollection<Uri> candidates, ISet<string> seen, string? raw)
    {
        var uri = Sanitize(raw);
        if (uri is null)
            return;

        if (seen.Add(uri.ToString()))
            candidates.Add(uri);
    }
}
