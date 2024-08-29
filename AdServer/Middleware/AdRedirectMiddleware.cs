using AdServer.Repositories.AdRepository;
using AdServer.Tracking;
using Microsoft.AspNetCore.Http;

namespace AdServer.Middleware
{
    public class AdRedirectMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAdRepository _adRepository;

        public AdRedirectMiddleware(RequestDelegate next, IAdRepository adRepository)
        {
            _next = next;
            _adRepository = adRepository;
        }

        public async Task Invoke(HttpContext context, AdTracker adTracker)
        {
            if (!context.Request.Path.StartsWithSegments("/adredirect"))
            {
                await _next.Invoke(context);
                return;
            }

            var adIdString = context.Request.Query["id"];
            if (!int.TryParse(adIdString, out var adId))
            {
                await _next.Invoke(context);
                return;
            }

            var ad = _adRepository.Get(adId);
            if (ad is null)
            {
                await _next.Invoke(context);
                return;
            }

            adTracker.AddClick(adId);
            context.Response.Redirect(ad.Url);
        }
    }
}
