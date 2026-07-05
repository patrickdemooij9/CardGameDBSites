using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Tournaments;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Business.Tournament;
using SkytearHorde.Business.Services.Search;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Entities.Enums;

namespace SkytearHorde.Business.Services
{
    public class TournamentService
    {
        private readonly TournamentRepository _tournamentRepository;
        private readonly ITournamentConnector[] _tournamentConnectors;
        private readonly CardService _cardService;
        private readonly DeckRepository _deckRepository;
        private readonly ISiteAccessor _siteAccessor;
        private readonly SettingsService _settingsService;

        public TournamentService(TournamentRepository tournamentRepository, IEnumerable<ITournamentConnector> tournamentConnectors, CardService cardService, DeckRepository deckRepository, ISiteAccessor siteAccessor, SettingsService settingsService)
        {
            _tournamentRepository = tournamentRepository;
            _tournamentConnectors = tournamentConnectors.ToArray();
            _cardService = cardService;
            _deckRepository = deckRepository;
            _siteAccessor = siteAccessor;
            _settingsService = settingsService;
        }

        public IEnumerable<Tournament> GetRecent(int count = 6) =>
            _tournamentRepository.GetRecent(count);

        public int GetPlayerCount(int tournamentId) =>
            _tournamentRepository.GetPlayerCount(tournamentId);

        public IEnumerable<TournamentEntrantSummary> GetTop8Entrants(int tournamentId) =>
            _tournamentRepository.GetTop8Entrants(tournamentId);

        public IEnumerable<MetaWinningDeck> GetRecentWinningDecks(int count, int leaderGroupId, int leaderSlotId)
        {
            var rows = _tournamentRepository.GetRecentWinningDecks(count, leaderGroupId, leaderSlotId).ToArray();
            var names = ResolveCardNames(rows.Where(r => r.LeaderCardId.HasValue).Select(r => r.LeaderCardId!.Value));

            return rows.Select(r => new MetaWinningDeck
            {
                TournamentId = r.TournamentId,
                TournamentName = r.TournamentName,
                TournamentDateUtc = r.TournamentDateUtc,
                ExternalUrl = r.ExternalUrl,
                PlayerName = r.PlayerName,
                DeckId = r.DeckId,
                DeckName = r.DeckName,
                LeaderName = r.LeaderCardId.HasValue ? names.GetValueOrDefault(r.LeaderCardId.Value) : null
            }).ToArray();
        }

        public IEnumerable<MetaLeaderStat> GetTopLeaders(int days, int take, int leaderGroupId, int leaderSlotId, int? tournamentId = null)
        {
            var from = DateTime.UtcNow.AddDays(-days);
            var rows = _tournamentRepository.GetTopLeaders(from, leaderGroupId, leaderSlotId, tournamentId).ToArray();
            var names = ResolveCardNames(rows.Select(r => r.LeaderCardId));

            return rows
                .Select(r => new MetaLeaderStat
                {
                    LeaderName = names.GetValueOrDefault(r.LeaderCardId) ?? "Unknown",
                    Wins = r.Wins,
                    Top8Count = r.Top8Count
                })
                .Where(it => it.LeaderName != "Unknown")
                .Take(take)
                .ToArray();
        }

        public IEnumerable<MetaPopularCard> GetPopularCards(int days, int take, int leaderGroupId, int leaderSlotId)
        {
            var from = DateTime.UtcNow.AddDays(-days);
            var totalWinningDecks = _tournamentRepository.GetWinningDeckCount(from);
            if (totalWinningDecks == 0) return Array.Empty<MetaPopularCard>();

            var rows = _tournamentRepository.GetPopularCardsInWinningDecks(from, leaderGroupId, leaderSlotId)
                .Take(take)
                .ToArray();
            var names = ResolveCardNames(rows.Select(r => r.CardId));

            return rows.Select(r => new MetaPopularCard
            {
                CardName = names.GetValueOrDefault(r.CardId) ?? "Unknown",
                Percentage = (int)Math.Round((double)r.DeckCount / totalWinningDecks * 100)
            }).ToArray();
        }

        private Dictionary<int, string> ResolveCardNames(IEnumerable<int> cardIds)
        {
            var result = new Dictionary<int, string>();
            foreach (var id in cardIds.Distinct())
            {
                var card = _cardService.Get(id);
                if (card != null)
                {
                    result[id] = card.DisplayName;
                }
            }
            return result;
        }

        public async Task<ImportTournamentResult> ImportTournament(ImportTournament model)
        {
            var connector = _tournamentConnectors.FirstOrDefault(c => c.Source == model.Source);
            if (connector is null) return new ImportTournamentResult { Success = false, Message = "Tournament connector not found" };

            var tournamentData = await connector.LoadTournament(model.ExternalId);
            if (tournamentData is null) return new ImportTournamentResult { Success = false, Message = "Tournament data not found" };

            var tournament = new Tournament
            {
                Name = tournamentData.Name,
                Type = model.Type,
                FormatId = model.FormatId,
                DateUtc = tournamentData.DateUtc,
                Source = model.Source,
                ExternalUrl = tournamentData.ExternalUrl,
                ExternalId = tournamentData.ExternalId
            };

            var otherData = await connector.GetData(tournament);
            if (otherData is null) return new ImportTournamentResult { Success = false, Message = "Failed to retrieve tournament data" };

            var deckSettings = _settingsService.GetSquadSettings(model.FormatId);
            var decks = CreateDecksFromData(deckSettings, otherData, out var missingCards);
            if (missingCards.Count > 0)
            {
                var message = $"Import failed due to missing cards: {string.Join(", ", missingCards)}";
                return new ImportTournamentResult { Success = false, Message = message, MissingCards = missingCards };
            }

            // Save tournament only after validation passes
            _tournamentRepository.Save(tournament);

            foreach (var round in otherData.RoundsByExternalId.Values)
            {
                round.TournamentId = tournament.Id;
                _tournamentRepository.Save(round);
            }

            // Create and save decks before entrants, so we can attach deck IDs
            var deckIdsByEntrantExternalId = new Dictionary<int, int>();
            foreach (var kvp in decks)
            {
                var entrantExternalId = kvp.Key;
                var deck = kvp.Value;

                _deckRepository.Create(deck);
                deckIdsByEntrantExternalId[entrantExternalId] = deck.Id;
            }

            // Attach deck IDs to entrants before saving
            foreach (var kvp in otherData.EntrantsByExternalId)
            {
                var entrantExternalId = kvp.Key;
                var entrant = kvp.Value;

                if (deckIdsByEntrantExternalId.TryGetValue(entrantExternalId, out var deckId))
                {
                    entrant.TournamentDeckId = deckId;
                }

                entrant.TournamentId = tournament.Id;
                _tournamentRepository.Save(entrant);
            }

            // At this point every round and entrant has a real DB Id assigned by Save().
            // Remap the external IDs stored on each match to the actual DB IDs.
            foreach (var match in otherData.Matches)
            {
                if (otherData.RoundsByExternalId.TryGetValue(match.RoundId, out var round))
                    match.RoundId = round.Id;

                if (match.Entrant1Id.HasValue &&
                    otherData.EntrantsByExternalId.TryGetValue(match.Entrant1Id.Value, out var e1))
                    match.Entrant1Id = e1.Id;

                if (match.Entrant2Id.HasValue &&
                    otherData.EntrantsByExternalId.TryGetValue(match.Entrant2Id.Value, out var e2))
                    match.Entrant2Id = e2.Id;

                if (otherData.EntrantsByExternalId.TryGetValue(match.WinnerEntrantId, out var winner))
                    match.WinnerEntrantId = winner.Id;

                _tournamentRepository.Save(match);
            }

            return new ImportTournamentResult { Success = true, Message = "Tournament imported successfully" };
        }

        private Dictionary<int, Deck> CreateDecksFromData(SquadSettings deckSettings, TournamentConnectorData data, out List<string> missingCards)
        {
            var decks = new Dictionary<int, Deck>();
            missingCards = [];

            var resolvedCards = new Dictionary<string, Entities.Models.Business.Card?>();

            foreach (var (entrantId, deckData) in data.DeckDataByEntrantExternalId)
            {
                if (deckData == null)
                    continue;

                var deck = new Deck(deckData.Name)
                {
                    SiteId = _siteAccessor.GetSiteId(),
                    TypeId = deckSettings.TypeID,
                    Source = DeckSource.TournamentSync,
                    IsPublished = true,
                    CreatedDate = DateTime.UtcNow,
                    Cards = [],
                    Sideboard = [],
                    IsLegal = false // TODO: Do we need to check this?
                };

                if (deckData.Cards == null || deckData.Cards.Count == 0)
                    continue;

                // TODO: Implement card lookup by name + optional subtitle
                // For now, cards will not be populated; decks are created as placeholders
                // Each MeleeDeckCard has: Lookup, Name, Subtitle, Quantity, Component (0=main, 99=sideboard), Type

                foreach (var card in deckData.Cards)
                {
                    // Search for the card by name
                    var searchQuery = card.Subtitle != null
                        ? $"{card.Name} {card.Subtitle}"
                        : card.Name;

                    resolvedCards.TryGetValue(searchQuery, out var resolvedCard);
                    if (!resolvedCards.ContainsKey(searchQuery))
                    {
                        var results = _cardService.Search(
                            new CardSearchQuery(int.MaxValue, _siteAccessor.GetSiteId())
                            {
                                Query = searchQuery,
                                VariantTypeIds = [0],
                                Amount = 1
                            }, out var _);

                        resolvedCard = results.FirstOrDefault();
                        resolvedCards[searchQuery] = resolvedCard;
                        if (resolvedCard is null)
                        {
                            // Card not found in local database; skip with silent warning
                            missingCards.Add($"{searchQuery} from {deckData.Name}");
                            continue;
                        }
                    }
                    if (resolvedCard is null) continue;

                    var isMainDeck = card.Component != 99;
                    var target = isMainDeck ? deck.Cards : deck.Sideboard;

                    // This is not really correct as we could have multiple groupings (See shatterpoint), but for now we just support SW-Unlimited
                    var grouping = isMainDeck ? deckSettings.Squads.ToItems<SquadConfig>().First() : deckSettings.SideboardGroup.ToItems<SquadConfig>().First();

                    var slotId = GetSlotForCard(grouping, resolvedCard);

                    // Use BaseId for the main card ID in the deck list
                    target.Add(new DeckCard(resolvedCard.BaseId, grouping.SquadId, slotId, card.Quantity));
                }
                decks[entrantId] = deck;
            }
            return decks;
        }

        private int GetSlotForCard(SquadConfig config, Entities.Models.Business.Card card)
        {
            var slots = config.Slots.ToItems<SquadSlotConfig>().ToArray();
            for (var i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot.Requirements.ToItems<ISquadRequirementConfig>().Where(it => it.RestrictionType != "Filter").All(r => r.GetRequirement().IsValid([card])))
                {
                    return i;
                }
            }
            return 0;
        }
    }
}
