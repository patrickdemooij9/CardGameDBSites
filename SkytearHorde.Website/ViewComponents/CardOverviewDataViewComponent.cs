using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Services.Search;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.ViewModels;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Entities.Models;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Models.Business;
using Card = SkytearHorde.Entities.Models.Business.Card;
using CardAttribute = SkytearHorde.Entities.Generated.CardAttribute;
using SkytearHorde.Entities.Models.ViewModels.DataSources;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;

namespace SkytearHorde.ViewComponents
{
    public class CardOverviewDataViewComponent : ViewComponent
    {
        private readonly ISiteService _siteService;
        private readonly ISiteAccessor _siteAccessor;
        private readonly CardService _cardService;
        private readonly ICardSearchService _searchService;
        private readonly CardPageService _cardPageService;
        private readonly CardPriceService _cardPriceService;
        private readonly SettingsService _settingsService;
        private readonly CollectionService _collectionService;
        private readonly MemberInfoService _memberInfoService;

        public CardOverviewDataViewComponent(ISiteService siteService, ISiteAccessor siteAccessor, CardService cardService, ICardSearchService searchService, CardPageService cardPageService, CardPriceService cardPriceService, SettingsService settingsService, CollectionService collectionService, MemberInfoService memberInfoService)
        {
            _siteService = siteService;
            _siteAccessor = siteAccessor;
            _cardService = cardService;
            _searchService = searchService;
            _cardPageService = cardPageService;
            _cardPriceService = cardPriceService;
            _settingsService = settingsService;
            _collectionService = collectionService;
            _memberInfoService = memberInfoService;
        }

        public IViewComponentResult Invoke(OverviewDataViewModel model)
        {
            var cardOverview = _siteService.GetCardOverview();
            var config = model.Config as CardOverviewDataSourceConfig ?? throw new ArgumentException();
            var viewModel = new CardOverviewDataModel();

            var sorting = new List<CardSorting>();
            if (!string.IsNullOrWhiteSpace(model.SortBy))
            {
                var selectedSorting = cardOverview.Sortings.ToItems<SortingItem>().FirstOrDefault(it => it.Value == model.SortBy);
                if (selectedSorting != null)
                {
                    sorting.Add(new CardSorting(selectedSorting.ExamineField!) { IsDescending = selectedSorting.Descending });
                }
            }
            else
            {
                var defaultSortOptions = _settingsService.GetSiteSettings().SortOptions;
                if (defaultSortOptions.All(it => it.Values?.Any() != true))
                {
                    sorting.AddRange(defaultSortOptions.Select(it => new CardSorting(it.ExamineField.IfNullOrWhiteSpace($"CustomField.{it.Ability?.Name}")) { IsDescending = it.Descending }));
                }
            }

            var filters = new List<FilterViewModel>();
            filters.AddRange(model.Config.Filters);
            filters.AddRange(model.Config.InternalFilters);

            var allCards = GetCards([.. filters], model.SearchQuery, sorting.ToArray(), config.VariantTypeId, model.PageNumber ?? 1, config.PageSize ?? int.MaxValue, out var totalCards);
            var ownedFilter = config.Filters.FirstOrDefault(it => it.Alias == "collection");

            viewModel.AbilitiesToShow = config.AttributesToShow;

            var prices = _settingsService.GetSiteSettings().AllowPricing ? _cardPriceService.GetPrices(allCards) : [];

            if (_settingsService.GetCollectionSettings().AllowCardCollecting && _memberInfoService.GetMemberInfo()?.IsLoggedIn is true)
            {
                viewModel.ShowCollection = true;

                viewModel.VariantTypes = _collectionService.GetVariantTypes()
                    .Select(it => new VariantTypeViewModel(it)).ToArray();
            }
            var collection = viewModel.ShowCollection ? _collectionService.GetCards().GroupBy(it => it.CardId).ToDictionary(it => it.Key, it => it.ToArray()) : [];
            var sets = _cardService.GetAllSets().ToDictionary(it => it.Id, it => it);

            var filteredCards = new List<CardItemViewModel>();
            foreach (var card in allCards)
            {
                if (config.IsValid?.Invoke(card) is false) continue;

                var isOwned = collection.TryGetValue(card.BaseId, out var collectionItems);
                if (ownedFilter != null)
                {
                    var checkedItem = ownedFilter.Items.FirstOrDefault(it => it.IsChecked);
                    if (checkedItem?.Value == "inCollection" && !isOwned) continue;
                    if (checkedItem?.Value == "none" && isOwned) continue;
                    if (checkedItem?.Value == "missing" && isOwned)
                    {
                        var amountOwned = collectionItems?.Sum(it => it.Amount) ?? 0;
                        var maxCard = card.GetAmount();
                        if (amountOwned >= maxCard) continue;
                    };
                }

                var variants = _collectionService.GetVariants(card, card.SetId).ToArray();
                CardCollectionViewModel? cardCollection = null;
                if (viewModel.ShowCollection)
                {
                    cardCollection = new CardCollectionViewModel();
                    foreach (var cardVariant in variants)
                    {
                        cardCollection.Amounts.Add(cardVariant.VariantId, collectionItems?.FirstOrDefault(it => it.VariantId == cardVariant.VariantId)?.Amount ?? 0);
                    }
                }

                var abilities = card.Attributes.Where(it => viewModel.AbilitiesToShow.ContainsKey(it.Key) || model.Config.Filters.Any(f => f.Alias.Equals(it.Key)));

                var hasSet = sets.TryGetValue(card.SetId, out var set);

                var variantId = card.VariantId;
                if (variantId == 0) //Temp until everything uses Examine
                {
                    variantId = variants.FirstOrDefault(it => it.VariantTypeId is null)?.VariantId ?? 0;
                }
                var cardPrice = prices.TryGetValue(variantId, out var value) ? value : null;

                filteredCards.Add(new CardItemViewModel(card.BaseId)
                {
                    SetId = card.SetId,
                    SetCode = hasSet ? set?.SetCode ?? string.Empty : string.Empty,
                    Name = card.DisplayName,
                    Url = _cardPageService.GetUrl(card, set),
                    Image = card.Image,
                    Price = cardPrice != null ? new PriceViewModel(cardPrice.MainPrice, _cardPriceService.GetUrl(cardPrice, card)) : null,
                    Collection = cardCollection,
                    Abilities = abilities.Select(it => new CardAbilityViewModel
                    {
                        Label = it.Value.GetAbilityValue(),
                        Value = it.Value.GetAbilityValue(),
                        Type = it.Key
                    }).Concat(new CardAbilityViewModel()
                    {
                        Label = "Expansion",
                        Value = card.SetName,
                        Type = "Set Name"
                    }.AsEnumerableOfOne()).ToArray(),
                    Variants = variants.Select(it => new CardVariantViewModel(it)).ToArray()
                });
            }

            CardItemViewModel[] cards;
            // TODO: This should also be handled by Examine in some way. This will certainly break after a few sets!
            if (model.SortBy?.Equals("collection") is true && viewModel.ShowCollection)
            {
                cards = filteredCards.OrderByDescending(it => it.Collection?.GetTotalAmount() ?? 0).ToArray();
            }
            else
            {
                cards = filteredCards.ToArray();
            }
            viewModel.Cards = new PagedResult<CardItemViewModel>(totalCards, model.PageNumber ?? 1, config.PageSize ?? int.MaxValue)
            {
                Items = cards
            };
            viewModel.Page = model.PageNumber ?? 1;
            viewModel.BaseUrl = cardOverview.Url();

            return View("/Views/Partials/components/cardOverviewData.cshtml", viewModel);
        }

        private Card[] GetCards(FilterViewModel[] filters, string? searchQuery, CardSorting[] orderby, int variantTypeId, int pageNumber, int pageSize, out int totalCards)
        {
            var filtersSelected = filters.Any(it => it.Items.Any(item => item.IsChecked));
            if (string.IsNullOrWhiteSpace(searchQuery) && orderby.Length == 0 && !filtersSelected)
            {
                var cards = _cardService.GetAll().Skip(pageSize * (pageNumber - 1)).Take(pageSize).ToArray();
                totalCards = cards.Length;
                return cards;
            }
            var query = new CardSearchQuery(pageSize, _siteAccessor.GetSiteId()) { Query = searchQuery, VariantTypeId = variantTypeId, Skip = pageSize * (pageNumber - 1) };
            if (orderby.Length > 0)
            {
                query.OrderBy.AddRange(orderby);
            }

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
            return _searchService.Search(query, out totalCards);
        }
    }
}
