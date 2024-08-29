using AdServer.Models;

namespace AdServer.Repositories.AdRepository
{
    public interface IAdRepository
    {
        Ad Get(int id);
        IEnumerable<Ad> GetAll();
        IEnumerable<Ad> GetByDomain(string domain);
    }
}
