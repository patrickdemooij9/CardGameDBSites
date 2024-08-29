namespace SkytearHorde.Entities.Models.PostModels
{
    public class AddCardCollectionPostModel
    {
        public int CardId { get; set; }
        public int NormalAmount { get; set; }
        public Dictionary<int, int> Variants { get; set; }

        public AddCardCollectionPostModel()
        {
            Variants = new Dictionary<int, int>();
        }
    }
}
