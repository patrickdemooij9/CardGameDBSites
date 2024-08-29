using SkytearHorde.Entities.Models.ViewModels;

namespace SkytearHorde.Business.Helpers
{
    public static class DeckCardGroupHelper
    {
        public static IEnumerable<DeckCardGroupViewModel[]> GroupByCardCount(DeckCardGroupViewModel[] items, int thresHold)
        {
            var amount = 0;
            var currentGroup = new List<DeckCardGroupViewModel>();

            foreach(var item in items)
            {
                if (item.Cards.Length == 0) continue;

                amount += item.Cards.Length;

                currentGroup.Add(item);
                if (amount >= thresHold)
                {
                    amount = 0;
                    yield return currentGroup.ToArray();
                    currentGroup = new List<DeckCardGroupViewModel>();
                }
            }
            yield return currentGroup.ToArray();
        }
    }
}
