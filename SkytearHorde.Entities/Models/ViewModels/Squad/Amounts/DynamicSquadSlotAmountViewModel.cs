namespace SkytearHorde.Entities.Models.ViewModels.Squad.Amounts
{
    public class DynamicSquadSlotAmountViewModel : SquadSlotAmountViewModel
    {
        public DynamicSquadSlotAmountViewModel(CreateSquadRequirement[] requirements)
        {
            Type = "dynamic";
            Config = new Dictionary<string, object>
            {
                { "requirements", requirements}
            };
        }
    }
}
