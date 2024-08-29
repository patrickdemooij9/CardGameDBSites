using AdServer.Repositories.AdRepository;
using AdServer.Repositories.CampaignRepository;
using AdServer.Repositories.MetricRepository;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using OfficeOpenXml;

namespace AdServer
{
    public class CampaignReportSender
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly IMetricRepository _metricRepository;
        private readonly IAdRepository _adRepository;

        public CampaignReportSender(ICampaignRepository campaignRepository,
            IMetricRepository metricRepository,
            IAdRepository adRepository)
        {
            _campaignRepository = campaignRepository;
            _metricRepository = metricRepository;
            _adRepository = adRepository;
        }

        public void CheckAndSendReports()
        {
            foreach(var campaign in _campaignRepository.GetCampaignsForReport())
            {
                var mail = new MimeMessage
                {
                    Subject = $"New ad report {campaign.NextReportScheduled!.Value.Date.ToShortDateString()}"
                };
                mail.From.Add(new MailboxAddress("Boardgame Ads", "info@skytearhordedb.com"));
                mail.To.Add(MailboxAddress.Parse(campaign.ReportMail));

                var body = new BodyBuilder();
                body.TextBody = "Attached you will find the current progress of your campaign. Please contact patrickdemooij9 on Discord if you spot any issues or have any questions.";
                body.Attachments.Add("CampaignData.xlsx", GetExcel(campaign));
                mail.Body = body.ToMessageBody();

                using var smtp = new SmtpClient();
                smtp.Connect("smtp.mijnhostingpartner.nl", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate("info@skytearhordedb.com", "Y00p$V1PRWbr");
                smtp.Send(mail);
                smtp.Disconnect(true);

                _campaignRepository.SetNextScheduledReport(campaign.Id, campaign.NextReportScheduled.Value.AddDays(1));
            }
        }

        private byte[] GetExcel(Models.Campaign campaign)
        {
            var ads = _adRepository.GetAll().Where(it => it.CampaignId == campaign.Id).ToDictionary(it => it.Id, it => it);
            var metrics = _metricRepository.GetByAds(ads.Keys.ToArray());

            using var package = new ExcelPackage();
            foreach (var groupedByAd in metrics.GroupBy(it => it.AdId))
            {
                var ad = ads[groupedByAd.Key];
                var worksheet = package.Workbook.Worksheets.Add(ad.Name);

                worksheet.Cells[1, 1].Value = "Date";
                worksheet.Cells[1, 2].Value = "Impressions";
                worksheet.Cells[1, 3].Value = "Clicks";

                var i = 2;
                foreach(var metricByDate in groupedByAd.GroupBy(it => it.Date).OrderBy(it => it.Key))
                {
                    worksheet.Cells[i, 1].Value = metricByDate.Key;
                    worksheet.Cells[i, 2].Value = metricByDate.Sum(it => it.TrackedImpressions);
                    worksheet.Cells[i, 3].Value = metricByDate.Sum(it => it.TrackedClicks);
                    i++;
                }

                worksheet.Tables.Add(worksheet.Cells[1, 1, groupedByAd.Count() + 1, 3], $"Data_{ad.Id}");
                worksheet.Cells.AutoFitColumns(0);
            }
            var memoryStream = new MemoryStream();
            return package.GetAsByteArray();
        }
    }
}
