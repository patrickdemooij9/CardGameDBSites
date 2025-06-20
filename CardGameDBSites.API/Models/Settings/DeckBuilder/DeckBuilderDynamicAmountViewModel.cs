using CardGameDBSites.API.Models.Settings.DeckBuilder;

namespace SkytearHorde.Entities.Models.ViewModels.Squad.Amounts
{
    public class DeckBuilderDynamicAmountViewModel : DeckBuilderSlotAmountApiModel
    {
        public DeckBuilderDynamicAmountViewModel(CreateSquadRequirement[] requirements): base("dynamic")
        {
            Config = new Dictionary<string, object>
            {
                { "requirements", requirements}
            };
        }
    }
}
