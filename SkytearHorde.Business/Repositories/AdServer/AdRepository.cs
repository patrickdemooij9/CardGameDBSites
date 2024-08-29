using AdServer.Models;
using AdServer.Repositories.AdRepository;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Entities.Generated;
using System.Linq;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories.AdServer
{
    public class AdRepository : IAdRepository
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public AdRepository(IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
        }

        public Ad Get(int id)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var ad = ctx.UmbracoContext.Content?.GetById(id) as AdServerAd;
            if (ad is null) return null;
            return Map(ad);
        }

        public IEnumerable<Ad> GetAll()
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var adRoot = ctx.UmbracoContext.Content?.GetAtRoot()?.FirstOrDefault(it => it is AdSettings);
            if (adRoot is null) return Enumerable.Empty<Ad>();
            return adRoot.Descendants<AdServerAd>().Select(Map).ToArray();
        }

        public IEnumerable<Ad> GetByDomain(string domain)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var adRoot = ctx.UmbracoContext.Content?.GetAtRoot()?.FirstOrDefault(it => it is AdSettings);
            if (adRoot is null) return Enumerable.Empty<Ad>();
            return adRoot.Descendants<AdServerAd>().Where(it => it.Domain.Equals(domain)).Select(Map).ToArray();
        }

        private Ad Map(AdServerAd item)
        {
            return new Ad
            {
                Id = item.Id,
                Name = item.Name,
                CampaignId = item.Parent.Id,
                Graphics = item.Graphics.ToItems<AdServerGraphic>().Select(it => new AdGraphic
                {
                    DesktopImageUrl = it.Desktop?.GetCropUrl(cropAlias: "Ad Desktop", width: 1200, height: 125),
                    TabletImageUrl = it.Tablet?.GetCropUrl(cropAlias: "Ad Tablet", width: 728, height: 90),
                    MobileImageUrl = it.Mobile?.GetCropUrl(cropAlias: "Ad Mobile", width: 320, height: 100)
                }).ToArray(),
                Url = item.Url,
                ShowAdExplanation = item.ShowAdExplanation
            };
        }
    }
}
