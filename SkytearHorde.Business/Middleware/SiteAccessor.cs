using Umbraco.Cms.Core.Cache;

namespace SkytearHorde.Business.Middleware
{
    public class SiteAccessor : ISiteAccessor
    {
        private static readonly AsyncLocal<int?> AmbientContext = new();

        private readonly IRequestCache _requestCache;

        public SiteAccessor(IRequestCache requestCache)
        {
            _requestCache = requestCache;
        }

        public int GetSiteId()
        {
            if (!_requestCache.IsAvailable)
            {
                return NonContextValue;
            }
            return (int)_requestCache.Get("SiteId")!;
        }

        public void SetSiteId(int siteId)
        {
            if (!_requestCache.IsAvailable)
            {
                NonContextValue = siteId;
                return;
            }
            _requestCache.Set("SiteId", siteId);
        }

        private int NonContextValue
        {
            get => AmbientContext.Value ?? default;
            set => AmbientContext.Value = value;
        }
    }
}
