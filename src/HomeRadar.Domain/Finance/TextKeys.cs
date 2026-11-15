using System.Text;

namespace HomeRadar.Domain.Finance;

public static class TextKeys
{
    private static readonly Dictionary<char, string> Folding = new()
    {
        ['à'] = "a",
        ['á'] = "a",
        ['â'] = "a",
        ['ä'] = "a",
        ['è'] = "e",
        ['é'] = "e",
        ['ê'] = "e",
        ['ë'] = "e",
        ['ì'] = "i",
        ['í'] = "i",
        ['î'] = "i",
        ['ï'] = "i",
        ['ò'] = "o",
        ['ó'] = "o",
        ['ô'] = "o",
        ['ö'] = "o",
        ['ù'] = "u",
        ['ú'] = "u",
        ['û'] = "u",
        ['ü'] = "u",
        ['ç'] = "c",
        ['ñ'] = "n",
        ['·'] = string.Empty,
        ['-'] = " ",
        ['\''] = " ",
        ['’'] = " ",
        ['/'] = " ",
        [','] = " ",
        ['.'] = " ",
    };

    public static string Words(string text)
    {
        var folded = Fold(text);
        var builder = new StringBuilder(folded.Length + 2).Append(' ');
        foreach (var character in folded)
        {
            builder.Append(char.IsLetterOrDigit(character) ? character : ' ');
        }

        return string.Join(' ', builder.Append(' ').ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public static bool ContainsPhrase(string words, string phrase)
    {
        var needle = Words(phrase);
        return needle.Length > 0 && $" {words} ".Contains($" {needle} ", StringComparison.Ordinal);
    }

    public static string Fold(string text)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var character in text.Trim().ToLowerInvariant())
        {
            if (Folding.TryGetValue(character, out var replacement))
            {
                builder.Append(replacement);
            }
            else
            {
                builder.Append(character);
            }
        }

        return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
