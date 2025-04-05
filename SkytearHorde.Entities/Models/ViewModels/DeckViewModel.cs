using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Models.ViewModels
{
    public class DeckViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsPublished { get; set; }
        public int TypeId { get; set; }
        public int AmountOfLikes { get; set; }
        public int Score { get; set; }

        public DeckViewModel(Deck deck)
        {
            Id = deck.Id;
            Name = deck.Name;
            Description = deck.Description;
            CreatedBy = deck.CreatedBy;
            CreatedDate = deck.CreatedDate;
            UpdatedDate = deck.UpdatedDate;
            IsPublished = deck.IsPublished;
            TypeId = deck.TypeId;
            AmountOfLikes = deck.AmountOfLikes;
            Score = deck.Score;
        }
    }
}
