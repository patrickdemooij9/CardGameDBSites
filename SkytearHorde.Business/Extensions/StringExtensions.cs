namespace SkytearHorde.Business.Extensions
{
    public static class StringExtensions
    {
        public static string ToSearchFriendly(this string str)
        {
            return str
                .Replace('-', ' ')
                .Replace("!", "")
                .Replace("?", "")
                .Replace(",", "")
                .Replace(":", "")
                .Replace(".", "")
                .Replace("…", "")
                .Replace('/', ' ')
                .Replace("\"", "")
                .Replace("\'", "")
                .Replace("“", "")
                .Replace("”", "")
                .Replace("…", "")
                .Replace("0", "O");
        }
    }
}
