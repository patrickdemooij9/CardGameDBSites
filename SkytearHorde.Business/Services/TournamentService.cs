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
        private readonly TournamentImportQueueRepository _importQueueRepository;
        private readonly PeriodRepository _periodRepository;
        private readonly ITournamentConnector[] _tournamentConnectors;
        private readonly CardService _cardService;
        private readonly DeckRepository _deckRepository;
        private readonly ISiteAccessor _siteAccessor;
        private readonly SettingsService _settingsService;
        private readonly MetaSnapshotService _metaSnapshotService;

        public TournamentService(TournamentRepository tournamentRepository, TournamentImportQueueRepository importQueueRepository, PeriodRepository periodRepository, IEnumerable<ITournamentConnector> tournamentConnectors, CardService cardService, DeckRepository deckRepository, ISiteAccessor siteAccessor, SettingsService settingsService, MetaSnapshotService metaSnapshotService)
        {
            _tournamentRepository = tournamentRepository;
            _importQueueRepository = importQueueRepository;
            _periodRepository = periodRepository;
            _tournamentConnectors = tournamentConnectors.ToArray();
            _cardService = cardService;
            _deckRepository = deckRepository;
            _siteAccessor = siteAccessor;
            _settingsService = settingsService;
            _metaSnapshotService = metaSnapshotService;
        }

        /// <summary>
        /// Validates and enqueues a tournament import for background processing. Returns immediately
        /// (no external calls); the actual import is performed later by <see cref="Business.BackgroundRunners.TournamentImportQueueTask"/>.
        /// A tournament that was already imported is re-synced (its data is replaced) rather than blocked;
        /// only an import that is already queued/processing is rejected, to avoid double-processing.
        /// </summary>
        public ImportTournamentResult QueueImport(ImportTournament model)
        {
            var connector = _tournamentConnectors.FirstOrDefault(c => c.Source == model.Source);
            if (connector is null) return new ImportTournamentResult { Success = false, Message = "Tournament connector not found" };

            var siteId = _siteAccessor.GetSiteId();

            if (_importQueueRepository.ExistsPending(siteId, model.Source, model.ExternalId))
                return new ImportTournamentResult { Success = false, Message = "Tournament import is already queued." };

            _importQueueRepository.Insert(new Entities.Models.Database.Tournament.TournamentImportQueueDBModel
            {
                SiteId = siteId,
                FormatId = model.FormatId,
                Type = model.Type,
                Source = model.Source,
                ExternalId = model.ExternalId,
                Status = Entities.Models.Database.Tournament.TournamentImportQueueStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });

            return new ImportTournamentResult { Success = true, Message = "Tournament import queued. It will be processed shortly." };
        }

        public IEnumerable<Tournament> GetRecent(int periodId, int count = 6) =>
            _tournamentRepository.GetRecent(_siteAccessor.GetSiteId(), periodId, count);

        public Tournament? GetById(int id) =>
            _tournamentRepository.GetById(id);

        public int GetPlayerCount(int tournamentId) =>
            _tournamentRepository.GetPlayerCount(tournamentId);

        public IEnumerable<TournamentEntrantSummary> GetTop8Entrants(int tournamentId, int leaderGroupId = 1, int leaderSlotId = 0) =>
            _tournamentRepository.GetTop8Entrants(tournamentId, leaderGroupId, leaderSlotId);

        public IReadOnlyList<TournamentAspectStat> GetAspectRepresentation(int tournamentId)
        {
            var rows = _tournamentRepository.GetDeckLeaderAndBaseCards(tournamentId);
            return CountAspectsPerDeck(rows.Select(r => new[] { r.LeaderCardId, r.BaseCardId }));
        }

        /// <summary>
        /// Which aspects the decks piloting a leader in a period pair with it. Counts the aspects of each
        /// deck's base card only — the leader's own aspects are identical on every one of those decks and
        /// would always read 100%.
        /// </summary>
        public IReadOnlyList<TournamentAspectStat> GetAspectRepresentationForLeader(int periodId, int leaderCardId, int leaderGroupId = 1, int leaderSlotId = 0)
        {
            var rows = _tournamentRepository.GetBaseCardsForLeader(_siteAccessor.GetSiteId(), periodId, leaderCardId, leaderGroupId, leaderSlotId);
            return CountAspectsPerDeck(rows.Select(r => new[] { r.BaseCardId }));
        }

        /// <summary>
        /// Percentage of decks featuring each aspect. A deck contributes the union of its cards' aspects,
        /// so it counts once per aspect however many of its cards carry that aspect.
        /// </summary>
        private TournamentAspectStat[] CountAspectsPerDeck(IEnumerable<IEnumerable<int?>> cardIdsPerDeck)
        {
            var aspectsByCard = new Dictionary<int, string[]>();
            string[] GetAspects(int cardId)
            {
                if (!aspectsByCard.TryGetValue(cardId, out var aspects))
                {
                    aspects = (_cardService.Get(cardId)?.GetMultipleCardAttributeValue("Aspects") ?? [])
                        // Values may come back comma-joined (e.g. "Vigilance,Villainy"); split and normalise.
                        .SelectMany(a => a.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        .Where(a => !a.Equals("None", StringComparison.OrdinalIgnoreCase))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();
                    aspectsByCard[cardId] = aspects;
                }
                return aspects;
            }

            var counts = new Dictionary<string, int>();
            var totalDecks = 0;
            foreach (var deck in cardIdsPerDeck)
            {
                totalDecks++;

                var deckAspects = new HashSet<string>();
                foreach (var cardId in deck)
                {
                    if (cardId.HasValue) deckAspects.UnionWith(GetAspects(cardId.Value));
                }

                foreach (var aspect in deckAspects)
                {
                    counts[aspect] = counts.GetValueOrDefault(aspect) + 1;
                }
            }

            if (totalDecks == 0) return [];

            return counts
                .Select(kv => new TournamentAspectStat
                {
                    Name = kv.Key,
                    Percentage = (int)Math.Round((double)kv.Value / totalDecks * 100)
                })
                .OrderByDescending(a => a.Percentage)
                .ThenBy(a => a.Name)
                .ToArray();
        }

        public IReadOnlyList<TournamentCardStat> GetMostPlayedCards(int tournamentId, int take)
        {
            var totalDecks = _tournamentRepository.GetTournamentDeckCount(tournamentId);
            if (totalDecks == 0) return [];

            return _tournamentRepository.GetMostPlayedCards(tournamentId, take)
                .Select(r => new TournamentCardStat
                {
                    CardId = r.CardId,
                    Percentage = (int)Math.Round((double)r.DeckCount / totalDecks * 100)
                })
                .ToArray();
        }

        /// <summary>Cards most often played alongside a leader in a period, as a percentage of that leader's decks.</summary>
        public IReadOnlyList<TournamentCardStat> GetMostPlayedCardsWithLeader(int periodId, int leaderCardId, int take, int leaderGroupId = 1, int leaderSlotId = 0)
        {
            var siteId = _siteAccessor.GetSiteId();
            var totalDecks = _tournamentRepository.GetDeckCountForLeader(siteId, periodId, leaderCardId, leaderGroupId, leaderSlotId);
            if (totalDecks == 0) return [];

            return _tournamentRepository.GetMostPlayedCardsWithLeader(siteId, periodId, leaderCardId, take, leaderGroupId, leaderSlotId)
                .Select(r => new TournamentCardStat
                {
                    CardId = r.CardId,
                    Percentage = (int)Math.Round((double)r.DeckCount / totalDecks * 100)
                })
                .ToArray();
        }

        public IEnumerable<MetaWinningDeck> GetRecentWinningDecks(int periodId, int count, int leaderGroupId, int leaderSlotId)
        {
            var rows = _tournamentRepository.GetRecentWinningDecks(_siteAccessor.GetSiteId(), periodId, count, leaderGroupId, leaderSlotId).ToArray();
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

        public IEnumerable<MetaLeaderStat> GetTopLeaders(int periodId, int take, int leaderGroupId, int leaderSlotId, int? tournamentId = null)
        {
            var rows = _tournamentRepository.GetTopLeaders(_siteAccessor.GetSiteId(), periodId, leaderGroupId, leaderSlotId, tournamentId).ToArray();
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

        public IEnumerable<MetaLeaderUsage> GetLeaderUsage(int tournamentId, int take, int leaderGroupId, int leaderSlotId)
        {
            var rows = _tournamentRepository.GetLeaderUsage(tournamentId, leaderGroupId, leaderSlotId).ToArray();
            var names = ResolveCardNames(rows.Select(r => r.LeaderCardId));

            return rows
                .Select(r => new MetaLeaderUsage
                {
                    LeaderName = names.GetValueOrDefault(r.LeaderCardId) ?? "Unknown",
                    Count = r.UsageCount
                })
                .Where(it => it.LeaderName != "Unknown")
                .Take(take)
                .ToArray();
        }

        public IEnumerable<MetaPopularCard> GetPopularCards(int periodId, int take, int leaderGroupId, int leaderSlotId)
        {
            var siteId = _siteAccessor.GetSiteId();
            var totalWinningDecks = _tournamentRepository.GetWinningDeckCount(siteId, periodId);
            if (totalWinningDecks == 0) return Array.Empty<MetaPopularCard>();

            var rows = _tournamentRepository.GetPopularCardsInWinningDecks(siteId, periodId, leaderGroupId, leaderSlotId)
                .Take(take)
                .ToArray();
            var names = ResolveCardNames(rows.Select(r => r.CardId));

            return rows.Select(r => new MetaPopularCard
            {
                CardId = r.CardId,
                CardName = names.GetValueOrDefault(r.CardId) ?? "Unknown",
                Percentage = (int)Math.Round((double)r.DeckCount / totalWinningDecks * 100)
            }).ToArray();
        }

        public IEnumerable<int> GetTopDeckIdsForLeader(int periodId, int cardId, int count, int leaderGroupId = 1, int leaderSlotId = 0) =>
            _tournamentRepository.GetTopDeckIdsForLeader(_siteAccessor.GetSiteId(), periodId, cardId, leaderGroupId, leaderSlotId, count);

        public IEnumerable<Period> GetPeriods(int formatId) =>
            _periodRepository.GetBySiteAndFormat(_siteAccessor.GetSiteId(), formatId);

        public Period? GetCurrentPeriod(int formatId) =>
            GetPeriods(formatId).Where(p => p.EndDateUtc == null).OrderByDescending(p => p.StartingDateUtc).FirstOrDefault();

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
                ExternalId = tournamentData.ExternalId,
                SiteId = _siteAccessor.GetSiteId()
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

            var matchingPeriod = _periodRepository.FindMatchingPeriod(tournament.SiteId, tournament.FormatId, tournament.DateUtc);
            tournament.PeriodId = matchingPeriod?.Id;

            var existing = _tournamentRepository.GetBySourceAndExternalId(tournament.SiteId, tournament.Source, tournament.ExternalId);
            var existingEntrantsByExternalId = new Dictionary<string, TournamentEntrant>();
            if (existing is not null)
            {
                tournament.Id = existing.Id;

                foreach (var oldEntrant in _tournamentRepository.GetEntrantsByTournamentId(existing.Id))
                {
                    if (!string.IsNullOrWhiteSpace(oldEntrant.ExternalId))
                        existingEntrantsByExternalId[oldEntrant.ExternalId] = oldEntrant;
                }

                // Rounds and matches are regenerated, so clear the old ones first.
                _tournamentRepository.DeleteRoundsAndMatches(existing.Id);
            }

            // Save tournament only after validation passes (insert when new, update when re-importing)
            _tournamentRepository.Save(tournament);

            foreach (var round in otherData.RoundsByExternalId.Values)
            {
                round.TournamentId = tournament.Id;
                _tournamentRepository.Save(round);
            }

            var deckIdsByEntrantExternalId = new Dictionary<int, int>();
            foreach (var kvp in decks)
            {
                var entrantExternalId = kvp.Key;
                var deck = kvp.Value;

                if (existingEntrantsByExternalId.TryGetValue(entrantExternalId.ToString(), out var existingWithDeck)
                    && existingWithDeck.TournamentDeckId is > 0)
                {
                    deck.Id = existingWithDeck.TournamentDeckId.Value;
                    // Only write a new version when the decklist actually changed.
                    var currentDeck = _deckRepository.Get(DeckStatus.Published, deck.Id).FirstOrDefault();
                    if (!deck.HasSameCards(currentDeck))
                        _deckRepository.Update(deck);
                    deckIdsByEntrantExternalId[entrantExternalId] = deck.Id;
                    continue;
                }

                _deckRepository.Create(deck);
                deckIdsByEntrantExternalId[entrantExternalId] = deck.Id;
            }

            // Upsert entrants: update the existing row (matched by external id) in place, or insert a new one.
            // Every entrant is saved — including players without a decklist — so that matches can safely
            // reference them (matches carry a FK to the entrant). Their deck is simply left null.
            foreach (var kvp in otherData.EntrantsByExternalId)
            {
                var entrantExternalId = kvp.Key;
                var entrant = kvp.Value;

                existingEntrantsByExternalId.TryGetValue(entrantExternalId.ToString(), out var existingEntrant);
                if (existingEntrant is not null)
                    entrant.Id = existingEntrant.Id; // preserve the entrant row (and any match references to it)

                if (deckIdsByEntrantExternalId.TryGetValue(entrantExternalId, out var deckId))
                {
                    entrant.TournamentDeckId = deckId;
                }
                else if (existingEntrant is not null)
                {
                    entrant.TournamentDeckId = existingEntrant.TournamentDeckId; // keep whatever deck it had (may be null)
                }
                else
                {
                    entrant.TournamentDeckId = null; // brand-new entrant without a decklist
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

            // Refresh the meta snapshot for this tournament's period so usage/winrate stay current.
            // Skipped when the tournament fell outside any configured period.
            if (tournament.PeriodId.HasValue)
            {
                _metaSnapshotService.RecomputeForPeriod(tournament.SiteId, tournament.FormatId, tournament.PeriodId.Value);
            }

            return new ImportTournamentResult { Success = true, Message = existing is not null ? "Tournament updated successfully" : "Tournament imported successfully" };
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

                    var isMainDeck = card.Component < 99;
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
