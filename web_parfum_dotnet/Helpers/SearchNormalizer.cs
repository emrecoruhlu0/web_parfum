using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace WebParfum.Helpers;

// PostgreSQL'deki perfume_normalize() ile birebir aynı sonucu üretir.
// Aksan kaldırma + lowercase + non-alphanumeric → space + multi-space tek space.
public static partial class SearchNormalizer
{
    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphaNumeric();

    [GeneratedRegex("\\s+")]
    private static partial Regex MultiSpace();

    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var formD = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        var s = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        s = NonAlphaNumeric().Replace(s, " ");
        s = MultiSpace().Replace(s, " ").Trim();
        return s;
    }

    public static string[] Tokenize(string? input)
    {
        var n = Normalize(input);
        return n.Length == 0
            ? []
            : n.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
}
