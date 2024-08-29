using SkytearHorde.Entities.Generated;

namespace SkytearHorde.Entities.Models.ViewModels
{
    public class CardViewModel
    {
        public string Title { get; set; }
        public string? Subtitle { get; set; }
        public string? Author { get; set; }
        public DateTime? Date { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }
        public CardTag[] Tags { get; set; }

        public CardViewModel()
        {
            Tags = Array.Empty<CardTag>();
        }
    }
}
