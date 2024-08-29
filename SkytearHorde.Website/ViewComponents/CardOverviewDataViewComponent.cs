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

            var sortingItem = string.IsNullOrWhiteSpace(model.SortBy) ? null : cardOverview.Sortings.ToItems<SortingItem>().FirstOrDefault(it => it.Value == model.SortBy);

            var filters = new List<FilterViewModel>();
            filters.AddRange(model.Config.Filters);
            filters.AddRange(model.Config.InternalFilters);

            var allCards = GetCards([.. filters], model.SearchQuery, sortingItem?.ExamineField, sortingItem?.Descending ?? false, config.VariantTypeId);
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

                var abilities = card.Attributes.Where(it => viewModel.AbilitiesToShow.ContainsKey(it.Key.Name) || model.Config.Filters.Any(f => f.Alias.Equals(it.Key.Name)));

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
                        Type = it.Key.Name
                    }).Concat(new CardAbilityViewModel()
                    {
                        Label = "Expansion",
                        Value = card.SetName,
                        Type = "Set Name"
                    }.AsEnumerableOfOne()).ToArray(),
                    Variants = variants.Select(it => new CardVariantViewModel(it)).ToArray()
                });
            }

            // TODO: This should also be handled by Examine in some way. This will certainly break after a few sets!
            if (model.SortBy?.Equals("collection") is true && viewModel.ShowCollection)
            {
                viewModel.Cards = filteredCards.OrderByDescending(it => it.Collection?.GetTotalAmount() ?? 0).ToArray();
            }
            else
            {
                viewModel.Cards = filteredCards.ToArray();
            }

            return View("/Views/Partials/components/cardOverviewData.cshtml", viewModel);
        }

        private Card[] GetCards(FilterViewModel[] filters, string? searchQuery, string? sortBy, bool sortDescending, int variantTypeId)
        {
            var filtersSelected = filters.Any(it => it.Items.Any(item => item.IsChecked));
            if (string.IsNullOrWhiteSpace(searchQuery) && string.IsNullOrWhiteSpace(sortBy) && !filtersSelected)
            {
                return _cardService.GetAll().ToArray();
            }
            var query = new CardSearchQuery(int.MaxValue, _siteAccessor.GetSiteId()) { Query = searchQuery, SortBy = sortBy, SortDescending = sortDescending, VariantTypeId = variantTypeId };
            foreach (var filter in filters)
            {
                if (filter.Alias == "collection") continue;

                var selectedValues = filter.Items.Where(it => it.IsChecked).ToArray();
                if (selectedValues.Length == 0) continue;

                query.CustomFields[filter.Alias] = selectedValues.Select(it => it.Value).ToArray();
            }
            return _searchService.Search(query);
        }
    }
}
