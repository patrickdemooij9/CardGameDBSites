using AdServer.Models;

namespace AdServer.Repositories.CampaignRepository
{
    public interface ICampaignRepository
    {
        Campaign[] GetCampaignsForReport();
        void SetNextScheduledReport(int campaignId, DateTime date);
    }
}
