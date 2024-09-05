using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Extensions;

namespace SkytearHorde.Entities.Models.Business
{
    public class Card
    {
        public int BaseId { get; set; }
        public int VariantId { get; set; }
        public int? VariantTypeId { get; set; }

        public required string DisplayName { get; set; }
        public required int SetId { get; set; }
        public required string SetName { get; set; }
        public required string UrlSegment { get; set; }
        public MediaWithCrops? Image { get; set; }
        public MediaWithCrops? BackImage { get; set; }
        public bool HideFromDecks { get; set; }
        public string? EmbedFooterText { get; set; }
        public DateTime CreatedDate { get; set; }
        public Dictionary<CardAttribute, IAbilityValue> Attributes { get; set; }
        public FrequentlyAskedQuestion[] Questions { get; set; }
        public CardSection[] Sections { get; set; }

        // Requirements
        public SlotTargetRequirement[] SlotTargetRequirements { get; set; }
        public ISquadRequirementConfig[] SquadRequirements { get; set; }
        public ISquadRequirementConfig[] TeamRequirements { get; set; }

        public Card(int id)
        {
            BaseId = id;
            Attributes = new Dictionary<CardAttribute, IAbilityValue>();
            Questions = Array.Empty<FrequentlyAskedQuestion>();
            Sections = Array.Empty<CardSection>();

            SlotTargetRequirements = Array.Empty<SlotTargetRequirement>();
            SquadRequirements = Array.Empty<ISquadRequirementConfig>();
            TeamRequirements = Array.Empty<ISquadRequirementConfig>();
        }

        public string[]? GetMultipleCardAttributeValue(string cardAttribute)
        {
            if (cardAttribute.Equals("Set Name")) return new string[] { SetName };

            foreach (var keyValue in Attributes)
            {
                if (keyValue.Key.Name.Equals(cardAttribute))
                {
                    return keyValue.Value.GetValues();
                }
            }
            return null;
        }

        public bool MatchesRequirements(ISquadRequirementConfig[] requirementConfig)
        {
            var cardArray = new[] { this };
            return requirementConfig.All(it => it.GetRequirement().IsValid(cardArray));
        }

        public int GetAmount()
        {
            var value = GetMultipleCardAttributeValue("Amount")?.FirstOrDefault();
            if (value != null && int.TryParse(value, out var amount))
                return amount;
            return 0;
        }

        public int GetCost()
        {
            var value = GetMultipleCardAttributeValue("Cost")?.FirstOrDefault();
            if (value != null && int.TryParse(value, out var cost))
                return cost;
            return 0;
        }

        // TODO: This is to make sure the collection manager can work with foil. I'll have to rework that a bit instead of cloning things.
        public Card Clone()
        {
            return new Card(BaseId)
            {
                VariantTypeId = VariantTypeId,
                DisplayName = DisplayName,
                SetId = SetId,
                SetName = SetName,
                UrlSegment = UrlSegment
            };
        }

        public static Card Map(Generated.Card card)
        {
            var set = card.Set?.OfType<Set>().FirstOrDefault();
            return new Card(card.Id)
            {
                DisplayName = card.DisplayName!,
                SetId = set?.Id ?? card.Parent!.Id,
                SetName = set?.Name ?? card.Parent!.Name,
                UrlSegment = card.UrlSegment!,
                Image = card.Image,
                BackImage = card.BackImage,
                HideFromDecks = card.HideFromDecks,
                EmbedFooterText = card.EmbedFooterText,
                Attributes = card.Attributes?.OfType<BlockListItem>()
                    .Select(it => it.Content as IAbilityValue)
                    .WhereNotNull()
                    .ToDictionary(it => new CardAttribute(it.GetAbility()), it => it) ?? new Dictionary<CardAttribute, IAbilityValue>(),
                Questions = card.Questions?.Select(it => (FrequentlyAskedQuestion)it.Content)?.ToArray() ?? [],
                Sections = card.Sections?.Select(it => (CardSection)it.Content)?.ToArray() ?? [],
                SlotTargetRequirements = card.SlotTargetRequirements?.Select(it => (SlotTargetRequirement)it.Content)?.ToArray() ?? [],
                SquadRequirements = card.SquadRequirements?.Select(it => (ISquadRequirementConfig)it.Content)?.ToArray() ?? [],
                TeamRequirements = card.TeamRequirements?.Select(it => (ISquadRequirementConfig)it.Content)?.ToArray() ?? []
            };
        }
    }
}
