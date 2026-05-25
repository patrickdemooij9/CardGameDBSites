using HtmlAgilityPack;
using SkytearHorde.Entities.Models.Business.Tournament;
using SkytearHorde.Entities.Models.Database.Tournament;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkytearHorde.Business.Tournaments
{
    public class MeleeGGTournamentConnector : ITournamentConnector
    {
        private readonly HttpClient _httpClient;

        public string Source => "melee.gg";

        private List<string> _matchesIds = [];

        public MeleeGGTournamentConnector(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
        }

        public async Task<Tournament?> LoadTournament(string externalId)
        {
            var tournamentPageResponse = await _httpClient.GetAsync($"https://melee.gg/Tournament/View/{externalId}");
            if (!tournamentPageResponse.IsSuccessStatusCode)
            {
                return null;
            }

            var doc = new HtmlDocument();
            doc.Load(await tournamentPageResponse.Content.ReadAsStreamAsync());

            var name = doc.DocumentNode.SelectSingleNode("//meta[@property='og:title']").GetAttributeValue("content", "");
            var dateTimeString = doc.DocumentNode.SelectSingleNode("//p[@id=\"tournament-headline-start-date-field\"]//span").GetAttributeValue("data-value", "");
            var matchesNodes = doc.DocumentNode.SelectNodes("//div[@id='pairings']//button");
            foreach (var matchNode in matchesNodes)
            {
                var matchId = matchNode.GetAttributeValue("data-id", "");
                if (!string.IsNullOrEmpty(matchId))
                {
                    _matchesIds.Add(matchId);
                }
            }
            return new Tournament
            {
                Name = name,
                DateUtc = DateTime.Parse(dateTimeString),
                Source = Source,
                ExternalId = externalId,
                ExternalUrl = $"https://melee.gg/Tournament/View/{externalId}"
            };
        }

        public async Task<TournamentConnectorData?> GetData(Tournament tournament)
        {
            var data = new TournamentConnectorData();
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            foreach (var matchId in _matchesIds)
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "draw", "3" },
                    { "start", "0" },
                    { "length", "1000" },
                    { "columns[0][data]", "TableNumber" },
                    { "columns[0][name]", "TableNumber" },
                    { "columns[0][searchable]", "true" },
                    { "columns[0][orderable]", "true" },
                    { "columns[0][search][value]", "" },
                    { "columns[0][search][regex]", "false" },
                    { "order[0][column]", "0" },
                    { "order[0][dir]", "asc" },
                    { "search[value]", "" },
                    { "search[regex]", "false" }
                });
                var matchResponse = await _httpClient.PostAsync($"https://melee.gg/Match/GetRoundMatches/{matchId}", content);
                if (!matchResponse.IsSuccessStatusCode)
                    continue;

                var json = await matchResponse.Content.ReadAsStringAsync();
                var response = JsonSerializer.Deserialize<MeleeMatchResponse>(json, jsonOptions);
                if (response?.Data == null)
                    continue;

                foreach (var matchData in response.Data)
                {
                    if (!data.RoundsByExternalId.TryGetValue(matchData.RoundId, out var round))
                    {
                        round = new TournamentRound
                        {
                            TournamentId = tournament.Id,
                            RoundNumber = matchData.RoundNumber,
                            Type = matchData.PhaseSortOrder > 1 ? RoundType.TopCut : RoundType.Swiss //TODO: I think this is wrong...
                        };
                        data.RoundsByExternalId[matchData.RoundId] = round;
                    }

                    int? entrant1ExternalId = null;
                    int? entrant2ExternalId = null;
                    int gamesWonP1 = 0;
                    int gamesWonP2 = 0;

                    for (int i = 0; i < matchData.Competitors.Count; i++)
                    {
                        var competitor = matchData.Competitors[i];
                        var player = competitor.Team?.Players?.FirstOrDefault();
                        if (player == null) continue;

                        if (!data.EntrantsByExternalId.TryGetValue(player.ID, out var entrant))
                        {
                            entrant = new TournamentEntrant
                            {
                                TournamentId = tournament.Id,
                                PlayerName = player.DisplayName,
                                ExternalId = player.ID.ToString(),
                                Source = Source
                            };
                            data.EntrantsByExternalId[player.ID] = entrant;
                        }

                        if (i == 0)
                        {
                            entrant1ExternalId = player.ID;
                            gamesWonP1 = competitor.GameWins ?? 0;
                        }
                        else
                        {
                            entrant2ExternalId = player.ID;
                            gamesWonP2 = competitor.GameWins ?? 0;
                        }
                    }

                    // Determine winner by highest GameWins; winner ID resolved to DB ID in TournamentService.
                    var winnerCompetitor = matchData.Competitors
                        .OrderByDescending(c => c.GameWins)
                        .FirstOrDefault();
                    var winnerExternalId = winnerCompetitor?.Team?.Players?.FirstOrDefault()?.ID ?? 0;

                    // Store external IDs temporarily; TournamentService will remap to real DB IDs after saving.
                    data.Matches.Add(new TournamentMatch
                    {
                        RoundId = matchData.RoundId,        // external — remapped in TournamentService
                        Entrant1Id = entrant1ExternalId,    // external — remapped in TournamentService
                        Entrant2Id = entrant2ExternalId,    // external — remapped in TournamentService
                        WinnerEntrantId = winnerExternalId, // external — remapped in TournamentService
                        GamesWonP1 = gamesWonP1,
                        GamesWonP2 = gamesWonP2
                    });
                }
            }

            return data;
        }

        // --- Melee.GG API response models ---

        private class MeleeMatchResponse
        {
            public List<MeleeMatch> Data { get; set; } = [];
        }

        private class MeleeMatch
        {
            public List<MeleeCompetitor> Competitors { get; set; } = [];
            public int RoundNumber { get; set; }
            public int RoundId { get; set; }
            public int PhaseSortOrder { get; set; }
        }

        private class MeleeCompetitor
        {
            public int ID { get; set; }
            public int TeamId { get; set; }
            public int? GameWins { get; set; }
            public string? ResultConfirmed { get; set; }
            public MeleeTeam? Team { get; set; }
        }

        private class MeleeTeam
        {
            public List<MeleePlayer> Players { get; set; } = [];
        }

        private class MeleePlayer
        {
            public int ID { get; set; }
            public int TeamId { get; set; }
            public string DisplayName { get; set; } = string.Empty;
        }
    }
}
