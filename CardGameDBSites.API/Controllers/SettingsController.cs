using CardGameDBSites.API.Models;
using CardGameDBSites.API.Models.Requirements;
using CardGameDBSites.API.Models.Settings;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SkytearHorde.Business.Exports;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Services;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
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

            return Ok(new SiteSettingsApiModel
            {
                MainColor = settings.MainColor,
                HoverMainColor = settings.HoverMainColor,
                BorderColor = settings.BorderColor,
                SiteName = settings.SiteName,
                ShowLogin = settings.ShowLogin,
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
                OverviewUrl = deckOverview.Url(),
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

        private NavigationItemApiModel MapNavigationItem(NavigationItem item)
        {
            var model = new NavigationItemApiModel
            {
                Name = item.Link?.Name!,
                Url = item.Link?.Url!,
                Children = [.. item.DropdownItems.ToItems<NavigationItem>().Select(MapNavigationItem)]
            };
            return model;
        }
    }
}
