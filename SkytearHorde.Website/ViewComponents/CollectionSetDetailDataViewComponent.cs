using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services.Search;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.ViewModels;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Extensions;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using SkytearHorde.Entities.Models.Business;
using Card = SkytearHorde.Entities.Models.Business.Card;

namespace SkytearHorde.ViewComponents
{
    public class CollectionSetDetailDataViewComponent : ViewComponent
    {
        private readonly ISiteAccessor _siteAccessor;
        private readonly CardService _cardService;
        private readonly CardPageService _cardPageService;
        private readonly SettingsService _settingsService;
        private readonly IMemberManager _memberManager;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly CollectionService _collectionService;

        public CollectionSetDetailDataViewComponent(ISiteAccessor siteAccessor, CardService cardService,  CardPageService cardPageService, SettingsService settingsService,
            IMemberManager memberManager, IUmbracoContextFactory umbracoContextFactory, CollectionService collectionService)
        {
            _siteAccessor = siteAccessor;
            _cardService = cardService;
            _cardPageService = cardPageService;
            _settingsService = settingsService;
            _memberManager = memberManager;
            _umbracoContextFactory = umbracoContextFactory;
            _collectionService = collectionService;
        }

        public IViewComponentResult Invoke(OverviewDataViewModel model)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var page = ctx.UmbracoContext.Content.GetById(model.PageId) as Set;
            if (page is null) throw new ArgumentException("No set found with given ID");

            var viewModel = new CollectionSetDetailViewModel();

            var ownedFilter = model.Config.Filters.FirstOrDefault(it => it.Alias == "collection");

            var allCards = GetCards([.. model.Config.Filters], model.SearchQuery, page.Id, model.SortBy is null ? [] : [new CardSorting(model.SortBy)]);
            var collectionCards = GetCollectionCards(allCards);

            var filteredCards = new List<CollectionCardItemViewModel>();
            foreach (var card in allCards)
            {
                var isOwned = collectionCards.TryGetValue(card.BaseId, out var collectionCard);
                if (ownedFilter != null)
                {
                    var checkedItem = ownedFilter.Items.FirstOrDefault(it => it.IsChecked);
                    if (checkedItem?.Value == "inCollection" && !isOwned) continue;
                    if (checkedItem?.Value == "missing" && isOwned)
                    {
                        var amountOwned = collectionCard?.VariantAmounts.Values.Sum() ?? 0;
                        var cardMax = card.GetAmount();
                        if (amountOwned > cardMax) continue;
                    };
                }

                var variants = _collectionService.GetVariants(card, card.SetId);
                var variantTypes = _collectionService.GetVariantTypes();
                var cardItemViewModel = new CardItemViewModel(card.BaseId)
                {
                    SetId = card.SetId,
                    Name = card.DisplayName,
                    Url = _cardPageService.GetUrl(card),
                    Image = card.Image,
                    Variants = variants.Select(it => new CardVariantViewModel(it)).ToArray()
                };
                var collectionCardViewModel = new CollectionCardItemViewModel(cardItemViewModel)
                {
                    VariantTypes = variantTypes.Select(it => new VariantTypeViewModel(it)).ToArray()
                };
                if (isOwned)
                {
                    collectionCardViewModel.Amounts = collectionCard.VariantAmounts;
                }

                filteredCards.Add(collectionCardViewModel);
            }

            viewModel.Cards = filteredCards.ToArray();
            viewModel.AllowCardCollecting = _memberManager.IsLoggedIn() && _settingsService.GetCollectionSettings().AllowCardCollecting;
            viewModel.VariantTypes = _collectionService.GetVariantTypes().Select(it => new VariantTypeViewModel(it)).ToArray();

            return View("/Views/Partials/components/collectionSetDetailData.cshtml", viewModel);
        }

        private Card[] GetCards(FilterViewModel[] filters, string? searchQuery, int setId, CardSorting[] sorting)
        {
            var filtersSelected = filters.Any(it => it.Items.Any(item => item.IsChecked));
            if (string.IsNullOrWhiteSpace(searchQuery) && !filtersSelected)
            {
                return _cardService.GetAll().ToArray();
            }
            var query = new CardSearchQuery(int.MaxValue, _siteAccessor.GetSiteId()) { Query = searchQuery, SetId = setId };
            if (sorting.Length > 0)
            {
                query.OrderBy.AddRange(sorting);
            }
            var searchFilters = new List<CardSearchFilter>();
            foreach (var filter in filters)
            {
                if (filter.Alias == "collection") continue;

                var selectedValues = filter.Items.Where(it => it.IsChecked).ToArray();
                if (selectedValues.Length == 0) continue;

                query.FilterClauses.Add(new CardSearchFilterClause
                {
                    Filters = [new CardSearchFilter
                    {
                        Alias = filter.Alias,
                        Values = selectedValues.Select(it => it.Value).ToArray()
                    }],
                    ClauseType = CardSearchFilterClauseType.AND
                });
            }
            return _cardService.Search(query, out _);
        }

        private Dictionary<int, CollectionCardItemGrouped> GetCollectionCards(Card[] cards)
        {
            if (!_settingsService.GetCollectionSettings().AllowCardCollecting || !_memberManager.IsLoggedIn())
            {
                return new Dictionary<int, CollectionCardItemGrouped>();
            }

            var cardsDict = cards.ToDictionary(it => it.BaseId, it => it);
            var result = new Dictionary<int, CollectionCardItemGrouped>();
            foreach (var cardGroup in _collectionService.GetCards().GroupBy(it => it.CardId))
            {
                if (!cardsDict.ContainsKey(cardGroup.Key)) { continue; }

                var collectionItemGrouped = new CollectionCardItemGrouped(cardGroup.Key);
                foreach (var variant in cardGroup.OrderBy(it => it.VariantId))
                {
                    collectionItemGrouped.VariantAmounts.Add(variant.VariantId, variant.Amount);
                }
                result.Add(cardGroup.Key, collectionItemGrouped);
            }
            return result;
        }
    }
}
