using SkytearHorde.Entities.Enums;

namespace SkytearHorde.Entities.Models.Business
{
    public class CollectionSetItem
    {
        public int Id { get; set; }
        public int SetId { get; set; }
        public int UserId { get; set; }
        public int Amount { get; set; }
    }
}
