using Examine;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Startup.Indexes;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using System.Collections.Concurrent;
using Umbraco.Extensions;
using Card = SkytearHorde.Entities.Models.Business.Card;

namespace SkytearHorde.Business.Services
{
    public class DeckLegalityService
    {
        private readonly CardService _cardService;
        private readonly DeckRepository _deckRepository;
        private readonly SettingsService _settingsService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly IExamineManager _examineManager;
        private readonly CardIndexValueSetBuilder _cardIndexValueSetBuilder;

        private readonly ConcurrentDictionary<int, string> _knownSetLegality = new();

        public DeckLegalityService(
            CardService cardService,
            DeckRepository deckRepository,
            SettingsService settingsService,
            ISiteAccessor siteAccessor,
            IExamineManager examineManager,
            CardIndexValueSetBuilder cardIndexValueSetBuilder)
        {
            _cardService = cardService;
            _deckRepository = deckRepository;
            _settingsService = settingsService;
            _siteAccessor = siteAccessor;
            _examineManager = examineManager;
            _cardIndexValueSetBuilder = cardIndexValueSetBuilder;
        }

        // A reprint in a set that is still legal keeps the card legal, so a set only counts when every printing is illegal.
        public static int[] ComputeNonLegalDeckTypes(int[] cardNonLegalDeckTypes, int[][] printingSetNonLegalDeckTypes)
        {
            var illegalInEveryPrinting = printingSetNonLegalDeckTypes.Length == 0
                ? []
                : printingSetNonLegalDeckTypes.Cast<IEnumerable<int>>().Aggregate((common, set) => common.Intersect(set));

            return [.. cardNonLegalDeckTypes.Union(illegalInEveryPrinting)];
        }

        public static bool ContainsIllegalCard(IEnumerable<int> cardIds, IReadOnlyDictionary<int, Card> cards, int deckTypeId)
        {
            return cardIds.Any(id => cards.TryGetValue(id, out var card) && !card.IsLegalFor(deckTypeId));
        }

        public static int[] GetIllegalCardIds(IEnumerable<Card> cards, int deckTypeId)
        {
            return [.. cards.Where(it => !it.IsLegalFor(deckTypeId)).Select(it => it.BaseId).Distinct()];
        }

        public static (int[] BecameLegal, int[] BecameIllegal, int IllegalDecks) FindChanges(IEnumerable<DeckLegalityVersion> versions, IEnumerable<int> illegalCardIds)
        {
            var illegalCards = illegalCardIds.ToHashSet();

            // The published version wins: a private draft must not change how the public deck is judged.
            var judged = versions
                .GroupBy(it => it.DeckId)
                .Select(deck => deck.OrderByDescending(it => it.Published).ThenByDescending(it => it.VersionId).First())
                .Select(version => (Version: version, ShouldBeLegal: !version.CardIds.Any(illegalCards.Contains)))
                .ToArray();

            return (
                [.. judged.Where(it => it.ShouldBeLegal && !it.Version.IsLegal).Select(it => it.Version.DeckId)],
                [.. judged.Where(it => !it.ShouldBeLegal && it.Version.IsLegal).Select(it => it.Version.DeckId)],
                judged.Count(it => !it.ShouldBeLegal));
        }

        public static bool HasBasePrintingIn(Card card, int setId)
        {
            return card.VariantReferences.Any(it => it.VariantTypeId is null && it.SetId == setId);
        }

        public DeckTypeLegalityResult[] Recalculate()
        {
            var siteId = _siteAccessor.GetSiteId();
            var cards = _cardService.GetAll().ToArray();
            var results = _settingsService.GetAllSquadSettings()
                .Select(it => RecalculateDeckType(siteId, it.TypeID, cards))
                .ToArray();

            _deckRepository.ClearAllCache();

            return results;
        }

        private DeckTypeLegalityResult RecalculateDeckType(int siteId, int typeId, Card[] cards)
        {
            var illegalCardIds = GetIllegalCardIds(cards, typeId);
            var versions = _deckRepository.GetCurrentVersionsForLegality(siteId, typeId);
            var (becameLegal, becameIllegal, illegalDecks) = FindChanges(versions, illegalCardIds);

            _deckRepository.SetLegal(becameLegal, true);
            _deckRepository.SetLegal(becameIllegal, false);

            return new DeckTypeLegalityResult
            {
                TypeId = typeId,
                IllegalCards = illegalCardIds.Length,
                BecameLegal = becameLegal.Length,
                BecameIllegal = becameIllegal.Length,
                IllegalDecks = illegalDecks
            };
        }

        // Compares against the last value seen, so the first publish after a restart reindexes once.
        public void ReindexCardsWhenSetLegalityChanged(Set set)
        {
            var legality = string.Join(',', set.NonLegalDeckTypes?.Select(it => it.Id).Order().ToArray() ?? []);
            if (_knownSetLegality.TryGetValue(set.Id, out var known) && known == legality) return;
            _knownSetLegality[set.Id] = legality;

            var siteId = set.Root().FirstChild<Settings>()?.FirstChild<SiteSettings>()?.SiteId;
            if (siteId is null) return;

            if (!_examineManager.TryGetIndex("CardIndex", out var index))
            {
                throw new InvalidOperationException("Could not obtain the card index");
            }

            _siteAccessor.SetSiteId(siteId.Value);

            var cards = _cardService.GetAll(true)
                .Where(it => it.VariantId != 0 && HasBasePrintingIn(it, set.Id))
                .ToArray();
            index.IndexItems(_cardIndexValueSetBuilder.GetValueSets(cards));
        }
    }
}
