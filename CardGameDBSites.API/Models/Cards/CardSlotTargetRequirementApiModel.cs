using CardGameDBSites.API.Models.Requirements;

namespace CardGameDBSites.API.Models.Cards
{
    public class CardSlotTargetRequirementApiModel
    {
        public int SlotId { get; set; }
        public RequirementApiModel[] Requirements { get; set; }

        public CardSlotTargetRequirementApiModel()
        {
            Requirements = [];
        }
    }
}
