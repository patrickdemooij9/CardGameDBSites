namespace SkytearHorde.Business.Services
{
    public static class AiPropertyKeyMatcher
    {
        private const string MultipleSuffix = "_multiple";

        public static string? FindAlias(string aiKey, IEnumerable<string> aliases)
        {
            var normalizedKey = Normalize(aiKey);
            return aliases.FirstOrDefault(alias => alias.Equals(aiKey))
                ?? aliases.FirstOrDefault(alias => Normalize(alias) == normalizedKey);
        }

        private static string Normalize(string key)
        {
            var withoutSuffix = key.EndsWith(MultipleSuffix, StringComparison.OrdinalIgnoreCase)
                ? key[..^MultipleSuffix.Length]
                : key;
            return new string(withoutSuffix.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        }
    }
}
