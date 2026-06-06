namespace SkytearHorde.Business.Tournaments
{
    /// <summary>
    /// DTO for deck data extracted from Melee.GG decklist API.
    /// Used to defer deck object creation to TournamentService where site/type context is available.
    /// </summary>
    public class MeleeDeckData
    {
        public string Name { get; set; } = string.Empty;
        public List<MeleeDeckCard> Cards { get; set; } = [];
    }
}
