using System.Globalization;

namespace gadifff.Mobile;

internal static class FullNameRules
{
    public const string ValidationMessage =
        "Enter at least a first and last name using letters, spaces, apostrophes, or hyphens.";

    public static bool TryNormalize(string? value, out string normalized)
    {
        normalized = Normalize(value);
        if (normalized.Length is 0 or > 100)
            return false;

        var parts = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 && parts.All(IsValidPart);
    }

    private static string Normalize(string? value)
    {
        var parts = (value ?? string.Empty)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        return string.Join(' ', parts.Select(NormalizePart));
    }

    private static bool IsValidPart(string part)
    {
        if (part.Length == 0 || !char.IsLetter(part[0]) || !char.IsLetter(part[^1]))
            return false;

        var previousWasSeparator = false;
        foreach (var character in part)
        {
            if (char.IsLetter(character))
            {
                previousWasSeparator = false;
                continue;
            }

            if (!IsNameSeparator(character) || previousWasSeparator)
                return false;

            previousWasSeparator = true;
        }

        return true;
    }

    private static string NormalizePart(string part)
    {
        var result = new char[part.Length];
        var capitalizeNext = true;

        for (var index = 0; index < part.Length; index++)
        {
            var character = part[index];
            if (char.IsLetter(character))
            {
                result[index] = capitalizeNext
                    ? char.ToUpper(character, CultureInfo.CurrentCulture)
                    : char.ToLower(character, CultureInfo.CurrentCulture);
                capitalizeNext = false;
            }
            else
            {
                result[index] = character;
                capitalizeNext = IsNameSeparator(character);
            }
        }

        return new string(result);
    }

    private static bool IsNameSeparator(char character) =>
        character is '-' or '\'' or '\u2019';
}
