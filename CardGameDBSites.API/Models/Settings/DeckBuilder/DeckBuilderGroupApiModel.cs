using CardGameDBSites.API.Models.Requirements;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.ViewModels.Squad;
using SkytearHorde.Entities.Models.ViewModels.Squad.Amounts;
using Umbraco.Cms.Core.Models.Blocks;

namespace CardGameDBSites.API.Models.Settings.DeckBuilder
{
    public class DeckBuilderGroupApiModel
    {
        public int Id { get; init; }
        public string? Name { get; set; }
        public RequirementApiModel[] Requirements { get; set; }
        public DeckBuilderSlotApiModel[] Slots { get; set; } = [];

        public DeckBuilderGroupApiModel()
        {
            Requirements = [];
        }

        public DeckBuilderGroupApiModel(SquadConfig config, bool isSideboard)
        {
            Id = config.SquadId;
            Name = config.Label;
            Requirements = [.. config.Requirements.ToItems<ISquadRequirementConfig>().Select(r => new RequirementApiModel(r))];
            Slots = [.. config.Slots.ToItems<SquadSlotConfig>().Select((slot, index) => new DeckBuilderSlotApiModel {
                Id = index,
                Name = slot.Label!,
                CardGroups = GetGroupsForSlot(slot),
                MinCards = slot.MinCards,
                MaxCardAmount = GetSlotAmount(slot.MaxCards),
                DisplaySize = slot.DisplaySize ?? "Small",
                DisableRemoval = slot.DisableRemoval,
                NumberMode = slot.NumberMode,
                AllowMovingToSideboard = slot.AllowMovingToSideboard,
                ShowIfTargetSlotIsFilled = slot.ShowIfTargetSlotIsFilled == 0 ? null : slot.ShowIfTargetSlotIsFilled - 1,
                Requirements = [.. slot.Requirements.ToItems<ISquadRequirementConfig>().Select(r => new RequirementApiModel(r))]
            })];
        }

        private DeckBuilderDeckCardGroupApiModel[] GetGroupsForSlot(SquadSlotConfig slot)
        {
            var groups = slot.Groupings.ToItems<DeckCardGroup>().ToArray();
            if (groups.Length == 0) // Always have a group to put items in
            {
                return [new DeckBuilderDeckCardGroupApiModel() { DisplayName = string.Empty }];
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
                return new DeckBuilderDynamicAmountViewModel(dynamicSquadSlotAmount.Requirements.ToItems<ISquadRequirementConfig>().Select(sr => new RequirementApiModel(sr)).ToArray());
            }

            return new DeckBuilderFixedAmountViewModel(0);
        }
    }
}
