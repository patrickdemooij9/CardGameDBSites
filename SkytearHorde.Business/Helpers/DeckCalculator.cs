using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Business.Helpers
{
    public class DeckCalculator
    {
        public int CalculateDeckScore(Deck deck, int[] last7DaysViews)
        {
            double score = 0;

            // 1. View velocity (recent days matter MUCH more)
            for (int i = 0; i < last7DaysViews.Length; i++)
            {
                // i = 0 is today → highest weight
                double weight = 1.0 / (i + 1);

                // Boost recent days more aggressively
                if (i == 0) weight *= 2;
                else if (i <= 2) weight *= 1.5;

                score += last7DaysViews[i] * weight;
            }

            // 2. Growth factor (are views increasing?)
            int recentViews = last7DaysViews.Take(2).Sum();
            int olderViews = last7DaysViews.TakeLast(2).Sum();

            if (olderViews > 0)
            {
                double growthRatio = (double)recentViews / olderViews;

                // Only reward real growth
                if (growthRatio > 1.2)
                {
                    score += growthRatio * 20;
                }
            }

            // 3. Recent likes (not total dominance)
            score += Math.Min(deck.AmountOfLikes, 50); // cap influence

            // 4. Strong recency boost (decays fast)
            var daysSinceCreation = (DateTime.UtcNow - deck.CreatedDate).TotalDays;

            if (daysSinceCreation <= 3)
                score += 50;
            else if (daysSinceCreation <= 7)
                score += 30;
            else if (daysSinceCreation <= 14)
                score += 10;

            // 5. Quality signals (light weight)
            if (!string.IsNullOrWhiteSpace(deck.Description))
                score += 5;

            if (deck.CreatedBy != null)
                score += 5;

            if (deck.Sideboard != null && deck.Sideboard.Count > 0)
                score += 5;

            return (int)Math.Ceiling(score);
        }
    }
}
