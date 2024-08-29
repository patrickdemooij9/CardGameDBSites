namespace SkytearHorde.Business.Middleware
{
    public interface ISiteAccessor
    {
        int GetSiteId();
        void SetSiteId(int siteId);
        bool HasSiteId();
    }
}
