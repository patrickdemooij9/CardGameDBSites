namespace CardGameDBSites.API.Models.Collection
{
    public class CollectionSettingsApiModel
    {
        public bool AllowSetCollecting { get; set; }
        public bool AllowCardCollecting { get; set; }
        public bool ShowProgressBar { get; set; }
        public int MissingCopiesThreshold { get; set; }
    }
}
