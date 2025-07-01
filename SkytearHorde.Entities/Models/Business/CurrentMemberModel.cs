namespace SkytearHorde.Entities.Models.Business
{
    public class CurrentMemberModel
    {
        public int Id { get; set; }
        public required string DisplayName { get; set; }
        public int[] LikedDecks { get; set; }
        public bool IsLoggedIn { get; set; }

        public CurrentMemberModel(int id)
        {
            Id = id;

            LikedDecks = Array.Empty<int>();
        }
    }
}
