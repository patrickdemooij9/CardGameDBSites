using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Interfaces
{
    public interface ISquadRequirement
    {
        string Alias { get; }

        bool IsValid(Card[] cards);
    }
}
