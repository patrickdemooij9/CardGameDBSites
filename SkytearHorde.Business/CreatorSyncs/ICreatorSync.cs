namespace SkytearHorde.Business.CreatorSyncs
{
    public interface ICreatorSync
    {
        IEnumerable<CreatorSyncItem> GetAll();
    }
}
