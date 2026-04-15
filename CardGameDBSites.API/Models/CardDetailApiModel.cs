using CardGameDBSites.API.Helpers;
using CardGameDBSites.API.Models.Cards;
using CardGameDBSites.API.Models.Requirements;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Entities.Generated;
using BusinessCard = SkytearHorde.Entities.Models.Business.Card;
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

        public int[] NonLegalDeckTypes { get; set; }

        public int[] AllowedChildren { get; set; }
        public int MaxChildren { get; set; }
        public CardDeckMutationApiModel[] Mutations { get; set; }
        public RequirementApiModel[] TeamRequirements { get; set; }
        public RequirementApiModel[] SquadRequirements { get; set; }
        public CardSlotTargetRequirementApiModel[] SlotTargetRequirements { get; set; }

        public CardPriceApiModel? Price { get; set; }
        public CardVariantReferenceApiModel[] Variants { get; set; }

        public CardDetailApiModel(BusinessCard card, int[] nonLegalDeckTypes)
        {
            BaseId = card.BaseId;
            VariantId = card.VariantId;
            VariantTypeId = card.VariantTypeId;
            DisplayName = card.DisplayName;
            SetId = card.SetId;
            SetName = card.SetName;
            UrlSegment = card.UrlSegment;
            ImageUrl = card.Image is null ? null : ImageCropHelper.ToApiModels(card.Image, "icon");
            BackImageUrl = card.BackImage is null ? null : ImageCropHelper.ToApiModels(card.BackImage, "icon");
            Attributes = card.Attributes.ToDictionary(it => it.Key, it => it.Value.GetValues());
            NonLegalDeckTypes = nonLegalDeckTypes;
            AllowedChildren = card.AllowedChildren ?? [];
            MaxChildren = card.MaxChildren;
            Mutations = [.. card.Mutations.Select(m => new CardDeckMutationApiModel { Alias = m.Alias, Change = m.Change, SlotId = m.SlotId })];
            TeamRequirements = [.. card.TeamRequirements.Select(r => new RequirementApiModel(r))];
            SquadRequirements = [.. card.SquadRequirements.Select(r => new RequirementApiModel(r))];
            SlotTargetRequirements = [.. card.SlotTargetRequirements.Select(r => new CardSlotTargetRequirementApiModel
            {
                SlotId = r.TargetSlotId,
                Requirements = [.. r.Requirements.ToItems<ISquadRequirementConfig>().Select(req => new RequirementApiModel(req))]
            })];

            Variants = [.. card.VariantReferences.Select(it => new CardVariantReferenceApiModel(it.VariantTypeId, it.CardVariantId, it.SetId))];
        }
    }
}
