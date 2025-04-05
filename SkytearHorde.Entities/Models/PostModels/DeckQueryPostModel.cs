using SkytearHorde.Entities.Enums;

namespace SkytearHorde.Entities.Models.PostModels
{
    public class DeckQueryPostModel
    {
        public int? TypeId { get; set; }
        public DeckStatus Status { get; set; }
        public int[] Cards { get; set; }
        public int Take { get; set; }
        public int Page { get; set; }
        public int? UserId { get; set; }
        public string? OrderBy { get; set; }

        public DeckQueryPostModel()
        {
            Cards = Array.Empty<int>();
        }
    }
}
