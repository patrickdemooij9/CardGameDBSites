namespace SkytearHorde.Entities.Models.ViewModels
{
    public class DeckLikeViewModel
    {
        public int DeckId { get; set; }
        public int AmountOfLikes { get; set; }
        public bool Like { get; set; }
        public bool IsAllowedToLike { get; set; }
    }
}
