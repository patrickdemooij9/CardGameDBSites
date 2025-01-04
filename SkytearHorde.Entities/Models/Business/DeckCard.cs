namespace SkytearHorde.Entities.Models.Business
{
    public class DeckCard
    {
        public int CardId { get; }
        public int GroupId { get; }
        public int SlotId { get; }
        public int Amount { get; set; }
        public List<DeckCardChild> Children { get; set; }

        public DeckCard(int cardId, int groupId, int slotId, int amount = 1)
        {
            CardId = cardId;
            GroupId = groupId;
            SlotId = slotId;
            Amount = amount;

            Children = new List<DeckCardChild>();
        }
    }
}
