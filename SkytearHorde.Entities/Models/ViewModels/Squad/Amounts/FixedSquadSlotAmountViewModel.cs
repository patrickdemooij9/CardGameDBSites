using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace SkytearHorde.Entities.Models.ViewModels.Squad.Amounts
{
    public class FixedSquadSlotAmountViewModel : SquadSlotAmountViewModel
    {
        public FixedSquadSlotAmountViewModel(int amount)
        {
            Type = "fixed";
            Config = new Dictionary<string, object>
            {
                { "amount", amount }
            };
        }
    }
}
