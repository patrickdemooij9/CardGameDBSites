namespace SkytearHorde.Business.Keywords
{
    public record KeywordBackfillCard(int CardId, string Name, Dictionary<string, string[]> Keywords);
}
