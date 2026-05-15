namespace SkytearHorde.Business.Services
{
    public class ParsedDeckCard
    {
        public int CardId { get; set; }
        public required string CardName { get; set; }
        public int Amount { get; set; }
        public required string Section { get; set; }
    }

    public class UnmatchedDeckCard
    {
        public required string CardName { get; set; }
        public int Amount { get; set; }
        public required string Section { get; set; }
    }

    public class ParseDeckTextResult
    {
        public List<ParsedDeckCard> Matched { get; set; } = new();
        public List<UnmatchedDeckCard> Unmatched { get; set; } = new();
    }

    public class DeckTextParser
    {
        private static readonly HashSet<string> SectionHeaders = new(StringComparer.OrdinalIgnoreCase)
        {
            "MainDeck", "Leader", "Base", "Sideboard"
        };

        private readonly CardService _cardService;

        public DeckTextParser(CardService cardService)
        {
            _cardService = cardService;
        }

        public ParseDeckTextResult Parse(string text)
        {
            var result = new ParseDeckTextResult();
            var currentSection = "MainDeck";

            // Build a name → baseId lookup (case-insensitive, includes full name with subtitle)
            var allCards = _cardService.GetAll().ToArray();
            var byFullName = allCards
                .GroupBy(c => c.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().BaseId, StringComparer.OrdinalIgnoreCase);

            foreach (var rawLine in text.Split('\n'))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Check section header
                if (SectionHeaders.Contains(line.Replace(" ", string.Empty)))
                {
                    currentSection = NormaliseSection(line);
                    continue;
                }

                // Parse "3 Card Name" or "Card Name"
                var (amount, cardName) = ParseCardLine(line);
                if (string.IsNullOrWhiteSpace(cardName)) continue;

                // Match card: try full name, then name-only (before " | ")
                if (byFullName.TryGetValue(cardName, out var cardId))
                {
                    result.Matched.Add(new ParsedDeckCard
                    {
                        CardId = cardId,
                        CardName = cardName,
                        Amount = amount,
                        Section = currentSection
                    });
                }
                else
                {
                    // Try matching just the base name (before " | ")
                    var pipeIdx = cardName.IndexOf(" | ", StringComparison.Ordinal);
                    if (pipeIdx > 0)
                    {
                        var baseName = cardName[..pipeIdx].Trim();
                        if (byFullName.TryGetValue(baseName, out var cardIdByBase))
                        {
                            result.Matched.Add(new ParsedDeckCard
                            {
                                CardId = cardIdByBase,
                                CardName = cardName,
                                Amount = amount,
                                Section = currentSection
                            });
                            continue;
                        }
                    }

                    result.Unmatched.Add(new UnmatchedDeckCard
                    {
                        CardName = cardName,
                        Amount = amount,
                        Section = currentSection
                    });
                }
            }

            return result;
        }

        private static (int amount, string cardName) ParseCardLine(string line)
        {
            var parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && int.TryParse(parts[0], out var amount))
            {
                return (amount, parts[1].Trim());
            }

            // No leading number
            return (1, line.Trim());
        }

        private static string NormaliseSection(string header)
        {
            // Map various spellings to canonical names
            var cleaned = header.Replace(" ", string.Empty);
            return cleaned.ToLowerInvariant() switch
            {
                "maindeck" => "MainDeck",
                "leader" => "Leader",
                "base" => "Base",
                "sideboard" => "Sideboard",
                _ => cleaned
            };
        }
    }
}
