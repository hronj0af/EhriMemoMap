using System.Text.RegularExpressions;

namespace EhriMemoMap.Helpers
{
    public static class StringHelpers
    {
        public static string Linkify(this string text)
        {
            // www|http|https|ftp|news|file
            text = Regex.Replace(
                text,
                @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])",
                "<a href=\"$1\" target=\"_blank\">$1</a>",
                RegexOptions.IgnoreCase)
                .Replace("href=\"www", "href=\"http://www");

            // mailto
            text = Regex.Replace(
                text,
                @"(([a-zA-Z0-9_\-\.])+@[a-zA-Z\ ]+?(\.[a-zA-Z]{2,6})+)",
                "<a href=\"mailto:$1\">$1</a>",
                RegexOptions.IgnoreCase);

            return text;
        }
    }
}
