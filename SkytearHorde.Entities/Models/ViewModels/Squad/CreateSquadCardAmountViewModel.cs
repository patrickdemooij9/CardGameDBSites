using Newtonsoft.Json;

namespace SkytearHorde.Entities.Models.ViewModels.Squad
{
    public class CreateSquadCardAmountViewModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("allowRemoval")]
        public bool AllowRemoval { get; set; }

        [JsonProperty("children")]
        public CreateSquadSlotViewModel[] Children { get; set; }

        public CreateSquadCardAmountViewModel()
        {
            Children = [];
        }
    }
}
