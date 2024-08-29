namespace SkytearHorde.Entities.Models.Business
{
    public class MemberModel
    {
        public int Id { get; set; }
        public int[] LikedDecks { get; set; }
        public bool IsLoggedIn { get; set; }

        public MemberModel(int id)
        {
            Id = id;

            LikedDecks = Array.Empty<int>();
        }
    }
}
