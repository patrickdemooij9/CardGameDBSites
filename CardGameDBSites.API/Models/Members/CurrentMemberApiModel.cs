namespace CardGameDBSites.API.Models.Members
{
    public class CurrentMemberApiModel
    {
        public required int Id { get; set; }
        public required string DisplayName { get; set; }
        public int[] LikedDecks { get; set; }
        public bool IsAdmin { get; set; }
        public int? ImpersonatedBy { get; set; }
    }
}
