namespace SkytearHorde.Entities.Models.Business
{
    public class DeckMutation
    {
        public required string Alias { get; set; }
        public int Change { get; set; }
        public int SlotId { get; set; }
    }
}
