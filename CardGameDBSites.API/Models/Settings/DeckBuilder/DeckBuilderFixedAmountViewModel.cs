using CardGameDBSites.API.Models.Settings.DeckBuilder;

namespace SkytearHorde.Entities.Models.ViewModels.Squad.Amounts
{
    public class DeckBuilderFixedAmountViewModel : DeckBuilderSlotAmountApiModel
    {
        public DeckBuilderFixedAmountViewModel(int amount) : base("fixed")
        {
            Config = new Dictionary<string, object>
            {
                { "amount", amount }
            };
        }
    }
}
