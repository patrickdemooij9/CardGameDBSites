using AdServer.Repositories.AdRepository;
using AdServer.Tracking;
using System.Web.Mvc;

namespace AdServer.Controllers
{
    public class AdRedirectController : Controller
    {
        private readonly IAdRepository _adRepository;
        private readonly AdTracker _adTracker;

        public AdRedirectController(IAdRepository adRepository, AdTracker adTracker)
        {
            _adRepository = adRepository;
            _adTracker = adTracker;
        }

        public ActionResult Click(int adId)
        {
            var ad = _adRepository.Get(adId);
            if (ad is null) return HttpNotFound();

            _adTracker.AddClick(adId);
            return Redirect(ad.Url);
        }
    }
}
