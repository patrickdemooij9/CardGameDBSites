using SkytearHorde.Entities.Models.Database;

namespace SkytearHorde.Business.Helpers
{
    public static class CardPriceDeltaCalculator
    {
        /// <summary>
        /// Calculates the delta for a brand-new price record being inserted.
        /// Returns the difference from the previous record's price, or 0 if there is no previous record.
        /// </summary>
        public static double CalculateDelta(double newMainPrice, CardPriceRecordDBModel? previousRecord)
        {
            return previousRecord != null ? newMainPrice - previousRecord.MainPrice : 0.0;
        }

        /// <summary>
        /// Recalculates the delta for an existing same-day record whose price is being updated.
        /// Derives the predecessor price from the existing record's stored delta and main price,
        /// then computes the new delta against that predecessor.
        /// </summary>
        public static double RecalculateDelta(double newMainPrice, CardPriceRecordDBModel existingRecord)
        {
            var predecessorPrice = existingRecord.MainPrice - existingRecord.Delta;
            return newMainPrice - predecessorPrice;
        }
    }
}
