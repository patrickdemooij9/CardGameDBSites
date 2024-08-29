namespace SkytearHorde.Entities.Models.PostModels
{
    public class AddToListPostModel
    {
        public int DeckId { get; set; }
        public int ExistingListId { get; set; }
        public string? NewListName { get; set; }
        public bool CreateNewList { get; set; }
    }
}
