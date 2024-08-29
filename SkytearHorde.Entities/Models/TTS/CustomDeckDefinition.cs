namespace SkytearHorde.Entities.Models.TTS
{
    public class CustomDeckDefinition : BaseObjectDefinition
    {
        public override string Name => "DeckCustom";

        public BaseObjectDefinition[] ContainedObjects { get; set; }
        public int[] DeckIDs { get; set; }
        public Dictionary<string, Card> CustomDeck { get; set; }
    }
}
