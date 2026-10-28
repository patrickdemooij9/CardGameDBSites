using System.Text.RegularExpressions;

namespace SkytearHorde.Business.Keywords
{
    public class KeywordDetector
    {
        private static readonly Regex ReminderText = new(@"\([^)]*\)", RegexOptions.Compiled);
        private static readonly Regex Whitespace = new(@"[ \t]+", RegexOptions.Compiled);
        private static readonly Regex SentenceBoundary = new(@"(?<=[.!?])\s+", RegexOptions.Compiled);
        private static readonly Regex KeywordListPrefix = new(@"^(\s*[A-Z][A-Za-z]*(\s+\d+)?\s*,)*\s*$", RegexOptions.Compiled);
        private static readonly Regex KeywordHeaderSuffix = new(@"^(\s*$|\s*\d|\s*\[|\s*,|\s*:|\s+[-–—]|\s+[A-Z])", RegexOptions.Compiled);
        private static readonly Regex GrantingKeywordPrefix = new(@"^\s*[A-Z][A-Za-z]*\s*[-–—]\s*$", RegexOptions.Compiled);
        private static readonly Regex AttachedUnit = new(@"\battached unit\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex TriggeredParagraph = new(@"^[^.:""]+:", RegexOptions.Compiled);
        private static readonly Regex GrantVerb = new(@"\b(gains?|gained|give|gives)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SynergyWord = new(@"\b(with|has|have|had|using|uses?|loses?|lost|without|named|ignores?|can't gain|cannot gain)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex Condition = new(@"\b(while|if|as long as|during|unless|for each|this phase|this attack|this round)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SelfReference = new(@"\b(this unit|this leader|this card|this base|he|she)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex OtherReference = new(@"\b(other|another|each|attached|friendly|enemy)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly char[] Quotes = ['"', '\'', '“', '”', '‘', '’'];

        private readonly (string Term, Regex Pattern)[] _keywords;
        private readonly (string Term, Regex Pattern)[] _triggers;
        private readonly string[] _ignoredPhrases;

        public KeywordDetector(KeywordDetectionConfig config)
        {
            _keywords = config.Keywords.Select(it => (it, TermPattern(it))).ToArray();
            _triggers = config.TriggeredAbilities.OrderByDescending(it => it.Length).Select(it => (it, TermPattern(it))).ToArray();
            _ignoredPhrases = [.. config.IgnoredPhrases];
        }

        public IReadOnlyCollection<DetectedKeyword> Detect(string cardName, IEnumerable<string> abilityTexts)
        {
            var ownName = cardName.Split(',')[0].Trim();
            var found = new HashSet<DetectedKeyword>();

            foreach (var paragraph in abilityTexts.SelectMany(SplitParagraphs))
            {
                var claimed = new List<Match>();
                foreach (var (term, pattern) in _keywords)
                {
                    foreach (var match in UnclaimedMatches(paragraph, pattern, claimed))
                    {
                        if (!char.IsUpper(paragraph[match.Index])) continue;

                        var relation = IsGrantedByKeyword(paragraph, match) ? KeywordRelation.Conditional
                            : IsKeywordHeader(paragraph, match) ? KeywordRelation.Has
                            : Classify(paragraph, match.Index, ownName);
                        found.Add(new DetectedKeyword(term, relation));
                    }
                }

                foreach (var (term, pattern) in _triggers)
                {
                    foreach (var match in UnclaimedMatches(paragraph, pattern, claimed))
                    {
                        if (IsGrantedByKeyword(paragraph, match))
                            found.Add(new DetectedKeyword(term, KeywordRelation.Conditional));
                        else if (IsTriggerHeader(paragraph, match))
                            found.Add(new DetectedKeyword(term, KeywordRelation.Has));
                        else if (IsAbilityReference(paragraph, match))
                            found.Add(new DetectedKeyword(term, Classify(paragraph, match.Index, ownName)));
                    }
                }
            }

            return found;
        }

        private static Regex TermPattern(string term)
        {
            return new Regex($@"(?<![\w-]){Regex.Escape(term)}(?![A-Za-z])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        // Terms are ordered longest first, so "Epic Action" claims its text before "Action" can.
        private static IEnumerable<Match> UnclaimedMatches(string paragraph, Regex pattern, List<Match> claimed)
        {
            foreach (Match match in pattern.Matches(paragraph))
            {
                if (claimed.Any(it => match.Index < it.Index + it.Length && it.Index < match.Index + match.Length)) continue;

                claimed.Add(match);
                yield return match;
            }
        }

        // "Coordinate - Saboteur": the leading keyword only hands out the ability while its condition holds.
        private static bool IsGrantedByKeyword(string paragraph, Match match)
        {
            return GrantingKeywordPrefix.IsMatch(paragraph[..match.Index]);
        }

        // Imported text mixes real line breaks with literal "\r" and the typo "/r".
        private IEnumerable<string> SplitParagraphs(string text)
        {
            var cleaned = _ignoredPhrases.Aggregate(text, (current, phrase) => current.Replace(phrase, "", StringComparison.OrdinalIgnoreCase));
            cleaned = ReminderText.Replace(cleaned, "")
                .Replace("|", "")
                .Replace("\\r", "\n")
                .Replace("/r", "\n")
                .Replace("\r", "\n");

            return cleaned.Split('\n')
                .Select(it => Whitespace.Replace(it, " ").Trim())
                .Where(it => it.Length > 0);
        }

        private static bool IsKeywordHeader(string paragraph, Match match)
        {
            var sentenceStart = SentenceBoundary.Matches(paragraph[..match.Index]).LastOrDefault() is { } boundary
                ? boundary.Index + boundary.Length
                : 0;
            return KeywordListPrefix.IsMatch(paragraph[sentenceStart..match.Index])
                && KeywordHeaderSuffix.IsMatch(paragraph[(match.Index + match.Length)..]);
        }

        private static bool IsTriggerHeader(string paragraph, Match match)
        {
            var colon = paragraph.IndexOf(':');
            if (colon < match.Index) return false;
            if (paragraph[..colon].IndexOfAny(Quotes) >= 0) return false;

            var before = paragraph[..match.Index].TrimEnd();
            return before.Length == 0 || before.EndsWith('/');
        }

        private static bool IsAbilityReference(string paragraph, Match match)
        {
            var before = paragraph[..match.Index].TrimEnd();
            var after = paragraph[(match.Index + match.Length)..].TrimStart();
            return after.StartsWith(':')
                || (before.Length > 0 && Quotes.Contains(before[^1]))
                || (after.Length > 0 && Quotes.Contains(after[0]));
        }

        private static KeywordRelation Classify(string paragraph, int index, string ownName)
        {
            var sentenceStart = SentenceBoundary.Matches(paragraph[..index]).LastOrDefault() is { } boundary
                ? boundary.Index + boundary.Length
                : 0;
            var sentenceEnd = SentenceBoundary.Match(paragraph, index) is { Success: true } end ? end.Index : paragraph.Length;
            var before = paragraph[sentenceStart..index];

            var grant = GrantVerb.Matches(before).LastOrDefault();
            var synergy = SynergyWord.Matches(before).LastOrDefault();
            // "can't gain" ends where its "gain" does, so a tie goes to synergy.
            if (grant is null || (synergy is not null && synergy.Index + synergy.Length >= grant.Index + grant.Length))
                return KeywordRelation.Synergy;

            var isGive = grant.Value.StartsWith("give", StringComparison.OrdinalIgnoreCase);
            var target = isGive
                ? before[(grant.Index + grant.Length)..]
                : before[..grant.Index].Split(',', ';', ':')[^1];

            var isOwn = (SelfReference.IsMatch(target) || target.Contains(ownName, StringComparison.OrdinalIgnoreCase))
                && !OtherReference.IsMatch(target)
                && !AttachedUnit.IsMatch(before);
            if (!isOwn) return KeywordRelation.Grants;

            var isConditional = Condition.IsMatch(paragraph[sentenceStart..sentenceEnd]) || TriggeredParagraph.IsMatch(paragraph[..index]);
            return isConditional ? KeywordRelation.Conditional : KeywordRelation.Has;
        }
    }
}
