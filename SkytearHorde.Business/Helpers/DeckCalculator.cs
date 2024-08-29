using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Business.Helpers
{
    public class DeckCalculator
    {
        public int CalculateDeckScore(Deck deck, int[] last7DaysViews)
        {
            double score = 0;
            if (!string.IsNullOrWhiteSpace(deck.Description))
            {
                score += GetPointsBasedOnDescription(deck.Description);
            }
            if (deck.CreatedBy != null)
            {
                score += 10;
            }
            for (int i = 0; i < last7DaysViews.Length; i++)
            {
                var points = GetPointsBasedOnViews(last7DaysViews[i]);
                score += points * (7 / (i + 1));
            }
            score += GetPointsBasedOnLikes(deck.AmountOfLikes);
            score += GetPointsBasedOnCreateDate(deck.CreatedDate);

            return (int)Math.Ceiling(score);
        }

        private int GetPointsBasedOnDescription(string description)
        {
            if (description.Length > 100)
            {
                return 20;
            }
            return 5;
        }

        private int GetPointsBasedOnViews(int views)
        {
            return views switch
            {
                int v when (v >= 100) => 20,
                int v when (v >= 50) => 10,
                int v when (v >= 10) => 5,
                int v when (v >= 1) => 1,
                _ => 0,
            };
        }

        private int GetPointsBasedOnLikes(int likes)
        {
            return likes * 2;
        }

        private int GetPointsBasedOnCreateDate(DateTime createdDate)
        {
            return createdDate.AddDays(30) > DateTime.UtcNow ? 5 : 0;
        }
    }
}
