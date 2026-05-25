using SkytearHorde.Entities.Models.Database.Tournament;

namespace SkytearHorde.Entities.Models.Business.Tournament
{
    public class TournamentRound
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }
        public int RoundNumber { get; set; }
        public RoundType Type { get; set; }
    }
}
