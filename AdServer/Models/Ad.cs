namespace AdServer.Models
{
    public class Ad
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CampaignId { get; set; }
        public AdGraphic[] Graphics { get; set; }
        public string Url { get; set; }
        public bool ShowAdExplanation { get; set; }

        public Ad()
        {
            Graphics = Array.Empty<AdGraphic>();
        }
    }
}
