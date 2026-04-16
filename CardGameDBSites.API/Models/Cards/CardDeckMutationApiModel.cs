namespace CardGameDBSites.API.Models.Cards
{
    public class CardDeckMutationApiModel
    {
        public required string Alias { get; set; }
        public int Change { get; set; }
        public int SlotId { get; set; }
    }
}
