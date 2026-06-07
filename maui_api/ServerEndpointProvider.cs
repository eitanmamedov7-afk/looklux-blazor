// SEARCH INDEX
// MAUI, MOBILE, SERVER, API, URL, ANDROID, EMULATOR, CONFIG
//
// Topic: MAUI SERVER ENDPOINT PROVIDER
// Purpose: Chooses and saves the backend API URL used by the MAUI app.
// Search keywords: MAUI MOBILE SERVER API URL ANDROID EMULATOR CONFIG
// When to use it: Show this when explaining why Android emulator uses http://10.0.2.2:7166.
// Important notes: It never calls the API; it only prepares candidate URLs for LookLuxApiClient.

namespace gadifff.Mobile;

internal static class ServerEndpointProvider
{
    // SECTION: SERVER URL CONFIG
    // Topic: Mobile backend URL selection
    // Purpose: Handles platform-specific API base URLs and saved custom URLs.
    // Search keywords: SERVER URL API ANDROID EMULATOR CONFIG
    // When to use it: Use when emulator connection issues happen.
    // Important notes: 10.0.2.2 is Android emulator's address for the host machine.
    private const string DefaultPort = "7166";
    private const string LegacyPort = "7164";
    private const string UrlPreferenceKey = "server_url";

    // FLOW_MAUI_SERVER_CONNECT_01: GetDefaultUrl chooses http://10.0.2.2:7166 for Android emulator backend access.
    // This file is involved because the emulator cannot use localhost to reach the PC; next step is startup candidate building.
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

    // FLOW_MAUI_SERVER_CONNECT_02: GetStartupCandidates builds saved/default/fallback backend URLs for MainPage.
    // This file is involved because MAUI must know where gadifff API is running; next step is MainPage.SetApiServer.
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
            uri = builder.Uri;
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
