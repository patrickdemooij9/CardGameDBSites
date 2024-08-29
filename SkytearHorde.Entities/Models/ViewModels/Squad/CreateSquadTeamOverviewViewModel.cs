using SkytearHorde.Entities.Generated;

namespace SkytearHorde.Entities.Models.ViewModels.Squad
{
    public class CreateSquadTeamOverviewViewModel
    {
        public CreateSquadTeamViewModel Team { get; set; }
        public List<FilterViewModel> Filters { get; set; }
        public List<CardDetailAbilityDisplay> Details { get; set; }

        public CreateSquadTeamOverviewViewModel(CreateSquadTeamViewModel team)
        {
            Team = team;
            Filters = new List<FilterViewModel>();
            Details = new List<CardDetailAbilityDisplay>();
        }
    }
}
