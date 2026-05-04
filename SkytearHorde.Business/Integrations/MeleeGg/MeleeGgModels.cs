using System.Text.Json.Serialization;

namespace SkytearHorde.Business.Integrations.MeleeGg
{
    // ── Tournament list ──────────────────────────────────────────────────────
    public class MeleeGgTournamentListResponse
    {
        [JsonPropertyName("items")]
        public List<MeleeGgTournamentSummary> Items { get; set; } = new();

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }
    }

    public class MeleeGgTournamentSummary
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("playerCount")]
        public int? PlayerCount { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    // ── Registrations (standings + decks) ────────────────────────────────────
    public class MeleeGgRegistration
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("player")]
        public MeleeGgPlayer? Player { get; set; }

        [JsonPropertyName("standing")]
        public MeleeGgStanding? Standing { get; set; }

        [JsonPropertyName("decklistId")]
        public string? DecklistId { get; set; }

        [JsonPropertyName("decklistUrl")]
        public string? DecklistUrl { get; set; }
    }

    public class MeleeGgPlayer
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class MeleeGgStanding
    {
        [JsonPropertyName("placement")]
        public int? Placement { get; set; }

        [JsonPropertyName("wins")]
        public int? Wins { get; set; }

        [JsonPropertyName("losses")]
        public int? Losses { get; set; }

        [JsonPropertyName("draws")]
        public int? Draws { get; set; }
    }
}
