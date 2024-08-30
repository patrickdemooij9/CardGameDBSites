namespace SkytearHorde.Entities.Models.PostModels
{
    public class RandomizerPostModel
    {
        public required int PageId { get; set; }
        public Dictionary<int, bool> Sets { get; set; }

        public RandomizerPostModel()
        {
            Sets = [];
        }
    }
}
