namespace SkytearHorde.Entities.Models.PostModels
{
    public class PackPostModel
    {
        public int SetId { get; set; }
        public PackItemPostModel[] Items { get; set; }

        public PackPostModel()
        {
            Items = Array.Empty<PackItemPostModel>();
        }
    }
}
