using CardGameDBSites.API.Helpers;
using CardGameDBSites.API.Models.Cards;
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
        public string SetCode { get; set; }
        public string UrlSegment { get; set; }
        public ImageCropsApiModel? ImageUrl { get; set; }
        public ImageCropsApiModel? BackImageUrl { get; set; }

        public Dictionary<string, string[]> Attributes { get; set; }

        public int[] AllowedChildren { get; set; }
        public int MaxChildren { get; set; }

        public int[] NonLegalDeckTypes { get; set; }

        public CardPriceApiModel? Price { get; set; }
        public CardVariantReferenceApiModel[] Variants { get; set; }

        public CardDetailApiModel(Card card, int[] nonLegalDeckTypes, string urlSegment)
        {
            BaseId = card.BaseId;
            VariantId = card.VariantId;
            VariantTypeId = card.VariantTypeId;
            DisplayName = card.DisplayName;
            SetId = card.SetId;
            SetName = card.SetName;
            UrlSegment = urlSegment;
            ImageUrl = card.Image is null ? null : ImageCropHelper.ToApiModels(card.Image, "icon");
            BackImageUrl = card.BackImage is null ? null : ImageCropHelper.ToApiModels(card.BackImage, "icon");
            Attributes = card.Attributes.ToDictionary(it => it.Key, it => it.Value.GetValues());
            AllowedChildren = card.AllowedChildren ?? [];
            MaxChildren = card.MaxChildren;
            NonLegalDeckTypes = nonLegalDeckTypes;

            Variants = [.. card.VariantReferences.Select(it => new CardVariantReferenceApiModel(it.VariantTypeId, it.CardVariantId, it.SetId))];
        }
    }
}
