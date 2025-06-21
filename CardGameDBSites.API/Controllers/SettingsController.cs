using CardGameDBSites.API.Models;
using CardGameDBSites.API.Models.Requirements;
using CardGameDBSites.API.Models.Settings;
using CardGameDBSites.API.Models.Settings.DeckBuilder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Exports;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.ViewModels.Squad;
using SkytearHorde.Entities.Models.ViewModels.Squad.Amounts;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace CardGameDBSites.API.Controllers
{
    [ApiController]
    [EnableCors("api")]
    [Route("/api/settings")]
    public class SettingsController : Controller
    {
        private readonly SettingsService _settingsService;
        private readonly ISiteService _siteService;

        public SettingsController(SettingsService settingsService, ISiteService siteService)
        {
            _settingsService = settingsService;
            _siteService = siteService;
        }

        [HttpGet("site")]
        [ProducesResponseType(typeof(SiteSettingsApiModel), 200)]
        public IActionResult GetSiteSettings()
        {
            var settings = _settingsService.GetSiteSettings();
            var cardSettings = _settingsService.GetCardSettings();

            var loginPageUrl = _siteService.GetRoot().FirstChild<Login>()?.Url(mode: UrlMode.Relative);
            var accountNavigation = _siteService.GetRoot().FirstChild<AccountPage>()?.Children().Select(it => new NavigationItemApiModel { Name = it.Name, Url = it.Url(mode: UrlMode.Relative) }).ToArray() ?? [];

            return Ok(new SiteSettingsApiModel
            {
                MainColor = settings.MainColor,
                HoverMainColor = settings.HoverMainColor,
                BorderColor = settings.BorderColor,
                SiteName = settings.SiteName,
                ShowLogin = settings.ShowLogin,
                LoginPageUrl = loginPageUrl,
                AccountNavigation = accountNavigation,
                Navigation = [.. settings.Navigation.Select(MapNavigationItem)],
                NavigationLogoUrl = settings.NavigationLogoUrl,
                TextColorWhite = settings.TextColorWhite,
                FooterText = settings.FooterText,
                FooterLinks = [.. settings.FooterLinks.Select(it => new LinkApiModel
                {
                    Name = it.Name!,
                    Url = it.Url!
                })],
                CardSections = [.. cardSettings.Display.ToItems<IPublishedElement>().Select(it => new CardSectionApiModel(it))]
            });
        }

        [HttpGet("deckType")]
        [ProducesResponseType(typeof(DeckTypeSettingsApiModel), 200)]
        public IActionResult GetDeckTypeSettings(int typeId)
        {
            var deckTypeSettings = _settingsService.GetSquadSettings(typeId);
            if (deckTypeSettings is null) return NotFound();

            var deckOverview = _siteService.GetDeckOverview(typeId);
            if (deckOverview is null) return NotFound();

            var deckDetail = deckOverview.FirstChild<DeckDetail>();
            if (deckDetail is null) return NotFound();

            var actions = new List<DeckActionApiModel>();
            if (deckDetail.EnableForcetable)
            {
                actions.Add(new DeckActionApiModel { DisplayName = "Try on ForceTable", Icon = "crown", Type = "ForceTable" });
            }
            foreach (var action in deckDetail.ExportTypes.ToItems<IDeckExportType>())
            {
                actions.Add(new DeckActionApiModel { DisplayName = action.DisplayName, Icon = action.IconName, Type = action.GetType().Name });
            }
            return Ok(new DeckTypeSettingsApiModel
            {
                OverviewUrl = deckOverview.Url(mode: UrlMode.Relative),
                DisplayName = deckTypeSettings.TypeDisplayName.IfNullOrWhiteSpace("Standard deck"),
                AmountOfSquadCards = deckTypeSettings.AmountOfSquadCards,
                Actions = [.. actions],
                Groupings = [.. deckDetail.Groups.ToItems<DeckCardGroup>()
                    .Select(it => new DeckCardGroupApiModel
                    {
                        Header = it.Header!,
                        HideAmount = it.HideAmount,
                        Requirements = [.. it.Conditions.ToItems<ISquadRequirementConfig>()
                            .Select(r => new RequirementApiModel(r))],
                        Sorting = [.. it.Sorting.ToItems<SortOption>().Select(s => s.Ability?.Name ?? "")]
                    })],
                ImageRules = [..deckTypeSettings.SpecialImageRule.ToItems<CardImage>()
                    .Select(it => new DeckCardImageRuleApiModel
                    {
                        ImageUrl = it.Image?.Url(mode: UrlMode.Absolute) ?? "#",
                        Requirements = [..it.Conditions.ToItems<ISquadRequirementConfig>().Select(r => new RequirementApiModel(r))]
                    })],
                MainCardRequirements = [.. deckTypeSettings.MainCard.ToItems<ISquadRequirementConfig>()
                    .Select(it => new RequirementApiModel(it))]
            });
        }

        [HttpGet("deckBuilder")]
        [ProducesResponseType(typeof(DeckBuilderApiModel), 200)]
        public IActionResult GetDeckBuilderSettings(int typeId)
        {
            var deckTypeSettings = _settingsService.GetSquadSettings(typeId);
            if (deckTypeSettings is null) return NotFound();

            return Ok(new DeckBuilderApiModel
            {
                Groups = [.. deckTypeSettings.Squads.ToItems<SquadConfig>().Select(it => new DeckBuilderGroupApiModel
                {
                    Id = it.SquadId,
                    Name = it.Label,
                    Requirements = [.. it.Requirements.ToItems<ISquadRequirementConfig>().Select(r => new RequirementApiModel(r))],
                    Slots = [.. it.Slots.ToItems<SquadSlotConfig>().Select((slot, index) => new DeckBuilderSlotApiModel {
                        Id = index,
                        Name = slot.Label!,
                        CardGroups = GetGroupsForSlot(slot),
                        MinCards = slot.MinCards,
                        MaxCardAmount = GetSlotAmount(slot.MaxCards),
                        DisableRemoval = slot.DisableRemoval,
                        NumberMode = slot.NumberMode,
                        ShowIfTargetSlotIsFilled = slot.ShowIfTargetSlotIsFilled == 0 ? null : slot.ShowIfTargetSlotIsFilled - 1,
                        Requirements = [.. slot.Requirements.ToItems<ISquadRequirementConfig>().Select(r => new RequirementApiModel(r))]
                    })]
                })]
            });
        }

        private DeckBuilderDeckCardGroupApiModel[] GetGroupsForSlot(SquadSlotConfig slot)
        {
            var groups = slot.Groupings.ToItems<DeckCardGroup>().ToArray();
            if (groups.Length == 0) // Always have a group to put items in
            {
                return [ new DeckBuilderDeckCardGroupApiModel() { DisplayName = string.Empty }];
            }

            return groups.Select(it => new DeckBuilderDeckCardGroupApiModel
            {
                DisplayName = it.Header!,
                SortBy = "Cost",
                Requirements = [.. it.Conditions.ToItems<ISquadRequirementConfig>().Select(r => new RequirementApiModel(r))]
            }).ToArray();
        }

        private DeckBuilderSlotAmountApiModel GetSlotAmount(BlockListModel? item)
        {
            var firstItem = item?.FirstOrDefault();
            if (firstItem is null) return new DeckBuilderFixedAmountViewModel(0);

            if (firstItem.Content is FixedSquadSlotAmount fixedSquadSlotAmount)
            {
                return new DeckBuilderFixedAmountViewModel(fixedSquadSlotAmount.Amount);
            }
            else if (firstItem.Content is DynamicSquadSlotAmount dynamicSquadSlotAmount)
            {
                return new DeckBuilderDynamicAmountViewModel(dynamicSquadSlotAmount.Requirements.ToItems<ISquadRequirementConfig>().Select(sr => new CreateSquadRequirement(sr)).ToArray());
            }

            return new DeckBuilderFixedAmountViewModel(0);
        }

        private NavigationItemApiModel MapNavigationItem(NavigationItem item)
        {
            var url = item.Link!.Url!;
            if (!url.StartsWith('/'))
            {
                url = new Uri(item.Link!.Url!).LocalPath;
            }
            var model = new NavigationItemApiModel
            {
                Name = item.Link?.Name!,
                Url = url,
                Children = [.. item.DropdownItems.ToItems<NavigationItem>().Select(MapNavigationItem)]
            };
            return model;
        }
    }
}
