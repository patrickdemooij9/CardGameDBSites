namespace CardGameDBSites.API.Models.Cards
{
    public class CardsQueryPostApiModel
    {
        public string? Query { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int? SetId { get; set; }
        public Dictionary<string, string[]> CustomFields { get; set; }

        public CardsQueryPostApiModel()
        {
            CustomFields = new Dictionary<string, string[]>();
        }
    }
}
