namespace SkytearHorde.Business.Keywords
{
    public record KeywordBackfillResult(int CardsChecked, bool DryRun, List<KeywordBackfillCard> ChangedCards);
}
