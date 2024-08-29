using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Models.ViewModels
{
    public class DeckCardGroupViewModel
    {
        public Deck Deck { get; set; }
        public DeckCardGroup Group { get; set; }
        public Business.Card[] Cards { get; set; }
        public SquadSettings SquadSettings { get; set; }
    }
}
