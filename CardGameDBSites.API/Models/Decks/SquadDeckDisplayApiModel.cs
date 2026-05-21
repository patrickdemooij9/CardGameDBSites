using SkytearHorde.Entities.Generated;

namespace CardGameDBSites.API.Models.Decks
{
    public class SquadDeckDisplayApiModel : IDeckDisplayApiModel
    {
        public string Type => "squadDeckDisplay";
        public int AmountOfSquadCards { get; }

        public SquadDeckDisplayApiModel(SquadDeckCard config)
        {
            AmountOfSquadCards = config.AmountOfSquadCards;
        }
    }
}
