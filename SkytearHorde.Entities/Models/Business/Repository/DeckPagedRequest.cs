using SkytearHorde.Entities.Enums;

namespace SkytearHorde.Entities.Models.Business.Repository
{
    public class DeckPagedRequest
    {
        public int SiteId { get; set; }
        public int? TypeId { get; set; }

        public DeckStatus Status { get; set; }
        public int[] Cards { get; set; }
        public int Take { get; set; }
        public int Page { get; set; }
        public int? UserId { get; set; }
        public string? OrderBy { get; set; }

        // Is used together with the collection Orderby
        public int? UseUserCollectionId { get; set; }
        //public CollectionCardItem[] CollectionItems { get; set; }

        public DeckPagedRequest(int? typeId = null)
        {
            TypeId = typeId;

            Status = DeckStatus.Published;
            Cards = Array.Empty<int>();
            Page = 1;
        }

        public bool ShouldBeCached()
        {
            return Cards.Length == 0 && OrderBy != "collection";
        }

        public string ToCacheKey()
        {
            return $"{SiteId}-{TypeId}-{Status}-{Take}-{Page}-{OrderBy}-{UserId}";
        }
    }
}
