using SkytearHorde.Entities.Models.Business;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace CardGameDBSites.API.Models
{
    public class CardDetailApiModel
    {
        public int BaseId { get; set; }
        public int VariantId { get; set; }
        public int? VariantTypeId { get; set; }

        public string DisplayName { get; set; }
        public int SetId { get; set; }
        public string SetName { get; set; }
        public string UrlSegment { get; set; }
        public string? ImageUrl { get; set; }
        public string? BackImageUrl { get; set; }

        public Dictionary<string, string[]> Attributes { get; set; }

        public CardDetailApiModel(Card card)
        {
            BaseId = card.BaseId;
            VariantId = card.VariantId;
            VariantTypeId = card.VariantTypeId;
            DisplayName = card.DisplayName;
            SetId = card.SetId;
            SetName = card.SetName;
            UrlSegment = card.UrlSegment;
            ImageUrl = $"https://aidalon-db.com{card.Image?.Url(mode: UrlMode.Relative)}";
            BackImageUrl = card.BackImage?.Url(mode: UrlMode.Absolute);
            Attributes = card.Attributes.ToDictionary(it => it.Key, it => it.Value.GetValues());
        }
    }
}
