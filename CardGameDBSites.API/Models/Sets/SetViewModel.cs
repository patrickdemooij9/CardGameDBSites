namespace CardGameDBSites.API.Models.Sets
{
    public class SetViewModel
    {
        public required int Id { get; set; }
        public required string DisplayName { get; set; }
        public required string UrlSegment { get; set; }
        public string? ImageUrl { get; set; }
        public string? Code { get; set; }
        public string[] ExtraInformation { get; set; }

        public SetViewModel()
        {
            ExtraInformation = [];
        }
    }
}
