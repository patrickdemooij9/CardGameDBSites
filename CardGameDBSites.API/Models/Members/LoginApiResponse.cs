namespace CardGameDBSites.API.Models.Members
{
    public class LoginApiResponse
    {
        public required string Token { get; set; }
        public required CurrentMemberApiModel CurrentMember { get; set; }
    }
}
