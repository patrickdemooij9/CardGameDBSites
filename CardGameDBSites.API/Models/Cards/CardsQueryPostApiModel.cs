namespace CardGameDBSites.API.Models.Cards
{
    public class CardsQueryPostApiModel
    {
        public string? Query { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int? SetId { get; set; }
        public int? VariantTypeId { get; set; }
        public CardsQueryFilterClauseApiModel[] FilterClauses { get; set; }

        public CardsQueryPostApiModel()
        {
            FilterClauses = [];
        }
    }
}
