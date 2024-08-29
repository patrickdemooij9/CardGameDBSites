using AdServer.Models;
using AdServer.Repositories.CampaignRepository;
using SkytearHorde.Entities.Generated;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Repositories.AdServer
{
    public class CampaignRepository : ICampaignRepository
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IContentService _contentService;

        public CampaignRepository(IUmbracoContextFactory umbracoContextFactory, IContentService contentService)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _contentService = contentService;
        }

        public Campaign[] GetCampaignsForReport()
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var adRoot = ctx.UmbracoContext.Content?.GetAtRoot()?.FirstOrDefault(it => it is AdSettings);
            if (adRoot is null) return Array.Empty<Campaign>();
            return adRoot.Children<AdServerCampaign>()
                .Where(it => it.SendReportDate != null && it.SendReportDate <= DateTime.UtcNow)
                .Select(it => new Campaign(it.Name)
                {
                    Id = it.Id,
                    NextReportScheduled = it.SendReportDate,
                    ReportMail = it.MailTo
                })
                .ToArray();
        }

        public void SetNextScheduledReport(int campaignId, DateTime date)
        {
            var campaign = _contentService.GetById(campaignId);
            if (campaign is null || campaign.ContentType.Alias != AdServerCampaign.ModelTypeAlias) return;

            campaign.SetValue("sendReportDate", date);
            _contentService.SaveAndPublish(campaign);
        }
    }
}
