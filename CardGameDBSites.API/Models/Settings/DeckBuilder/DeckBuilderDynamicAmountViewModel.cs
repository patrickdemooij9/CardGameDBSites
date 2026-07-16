using CardGameDBSites.API.Models.Requirements;
using CardGameDBSites.API.Models.Settings.DeckBuilder;

namespace SkytearHorde.Entities.Models.ViewModels.Squad.Amounts
{
    public class DeckBuilderDynamicAmountViewModel : DeckBuilderSlotAmountApiModel
    {
        public DeckBuilderDynamicAmountViewModel(RequirementApiModel[] requirements): base("dynamic")
        {
            Config = new Dictionary<string, object>
            {
                { "requirements", requirements}
            };
        }
    }
}
