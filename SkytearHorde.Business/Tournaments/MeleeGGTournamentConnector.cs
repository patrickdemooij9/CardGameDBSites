using HtmlAgilityPack;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Business.Tournament;
using SkytearHorde.Entities.Models.Database.Tournament;
using System.Net.Http.Json;
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

            var lastRoundStandings = await GetRoundStandings(_matchesIds.Last());

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
                            var standing = lastRoundStandings.TryGetValue(player.ID, out var roundStanding) ? roundStanding : null;

                            entrant = new TournamentEntrant
                            {
                                TournamentId = tournament.Id,
                                PlayerName = player.DisplayName,
                                ExternalId = player.ID.ToString(),
                                Source = Source,
                                Placement = standing?.Rank ?? 1000,
                                Wins = standing?.GameWins ?? 0,
                                Losses = standing?.GameLosses ?? 0,
                                Draws = standing?.GameDraws ?? 0
                            };
                            data.EntrantsByExternalId[player.ID] = entrant;

                            // Load the entrant's deck from Melee if available
                            if (competitor.Decklists != null && competitor.Decklists.Length > 0)
                            {
                                var firstDecklistGuid = competitor.Decklists[0].DeckListId;
                                var deckData = await LoadDeckFromMelee(firstDecklistGuid.ToString());
                                if (deckData != null)
                                {
                                    data.DeckData.Add(deckData);
                                    data.DeckDataByEntrantExternalId[player.ID] = deckData;
                                }
                            }
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
                        GamesWonP2 = gamesWonP2,
                    });
                }
            }

            return data;
        }

        private async Task<MeleeDeckData?> LoadDeckFromMelee(string decklistGuid)
        {
            try
            {
                var deckUrl = $"https://melee.gg/Decklist/GetDecklistDetails?id={decklistGuid}";
                var response = await _httpClient.GetAsync(deckUrl);
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var decklistResponse = JsonSerializer.Deserialize<MeleeDecklistResponse>(json, jsonOptions);
                if (decklistResponse is null)
                    return null;

                // Return deck data; TournamentService will create the actual Deck object with site/type context
                return new MeleeDeckData
                {
                    Name = decklistResponse.DecklistName,
                    Cards = decklistResponse.Records ?? []
                };
            }
            catch
            {
                return null;
            }
        }

        private async Task<Dictionary<int, MeleeRoundStanding>> GetRoundStandings(string roundId)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "draw", "1" },
                { "start", "0" },
                { "length", "1000" },
                { "columns[0][data]", "Rank" },
                { "columns[0][name]", "Rank" },
                { "columns[0][searchable]", "true" },
                { "columns[0][orderable]", "true" },
                { "columns[0][search][value]", "" },
                { "columns[0][search][regex]", "false" },
                { "order[0][column]", "0" },
                { "order[0][dir]", "asc" },
                { "search[value]", "" },
                { "search[regex]", "false" },
                { "roundId", roundId }
            });
            var matchResponse = await _httpClient.PostAsync($"https://melee.gg/Standing/GetRoundStandings", content);
            if (!matchResponse.IsSuccessStatusCode)
                return [];

            var response = await matchResponse.Content.ReadFromJsonAsync<MeleeRoundResponse>();
            if (response?.Data == null)
                return [];

            return response.Data.ToDictionary(s => s.Team?.Players?.FirstOrDefault()?.ID ?? 0, s => s);
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
            public MeleeCompetitorDeckList[] Decklists { get; set; } = [];
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

        private class MeleeCompetitorDeckList
        {
            public Guid DeckListId { get; set; }
        }

        // --- Melee Decklist Detail Response Models ---

        private class MeleeDecklistResponse
        {
            public Guid Guid { get; set; }
            public string DecklistName { get; set; } = string.Empty;
            public string FormatName { get; set; } = string.Empty;
            public string Game { get; set; } = string.Empty;
            public List<MeleeDeckCard> Records { get; set; } = [];
        }

        private class MeleeRoundResponse
        {
            public List<MeleeRoundStanding> Data { get; set; } = [];
        }

        private class MeleeRoundStanding
        {
            public int GameWins { get; set; }
            public int GameLosses { get; set; }
            public int GameDraws { get; set; }
            public int Rank { get; set; }
            public MeleeTeam? Team { get; set; }
        }
    }
}
