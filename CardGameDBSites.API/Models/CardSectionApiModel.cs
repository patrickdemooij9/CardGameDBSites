using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace CardGameDBSites.API.Models
{
    public class CardSectionApiModel
    {
        public bool IsDivider { get; set; }

        public string Ability { get; set; }
        public bool ShowAsTags { get; set; }
        public string? OverviewPageUrl { get; set; }
        public string NamePosition { get; set; }

        public CardSectionApiModel(IPublishedElement publishedElement)
        {
            if (publishedElement is CardDetailAbilityDisplay display)
            {
                Ability = display.Ability?.Name;
                ShowAsTags = display.ShowAsTags;
                OverviewPageUrl = display.OverviewPage?.Url(mode: UrlMode.Absolute);
                NamePosition = display.NamePosition.IfNullOrWhiteSpace("inline");
            }
            else
            {
                IsDivider = true;
            }
        }
    }
}
