using AdServer.Models;
using AdServer.Repositories.AdRepository;

namespace AdServer.Finder
{
    public class AdFinder
    {
        private readonly IAdRepository _adRepository;

        public AdFinder(IAdRepository adRepository)
        {
            _adRepository = adRepository;
        }
        
        public Ad? GetAdToDisplay(string domain)
        {
            return _adRepository.GetByDomain(domain).FirstOrDefault(); //TODO: Should be different
        }
    }
}
