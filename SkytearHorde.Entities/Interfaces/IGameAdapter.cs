using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Interfaces
{
    public interface IGameAdapter
    {
        int SiteId { get; }
        int? FormatId { get; }
        object ExtractKeyAttributes(Deck deck);
        string SuggestArchetype(Deck deck);
    }
}
