using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Business.Exports.Collection
{
    public interface ICollectionExport
    {
        Task<byte[]> Export(CollectionCardItem[] collection);
        CollectionCardItem[] Import(Stream data);
    }
}
