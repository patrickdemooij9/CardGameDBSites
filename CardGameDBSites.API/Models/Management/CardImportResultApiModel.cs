namespace CardGameDBSites.API.Models.Management
{
    public class CardImportResultApiModel
    {
        public int TotalCardsFetched { get; set; }
        public int CardsCreated { get; set; }
        public int CardsDuplicate { get; set; }
        public int CardsSkipped { get; set; }
        public int CardsError { get; set; }
        public string Message { get; set; } = "Import started";
    }
}
