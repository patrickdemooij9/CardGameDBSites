using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Business.Exports
{
    public interface IDeckExport
    {
        Task<byte[]> ExportDeck(Deck deck);
    }
}
