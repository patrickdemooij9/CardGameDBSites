namespace SkytearHorde.Entities.Models.TTS
{
    public class CardDefinition : BaseObjectDefinition
    {
        public override string Name => "Card";

        public int CardId { get; set; }
        public Dictionary<string, Card> CustomDeck { get; set; }
    }
}
