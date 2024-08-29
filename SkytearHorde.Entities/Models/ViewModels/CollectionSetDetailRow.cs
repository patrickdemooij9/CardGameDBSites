using SkytearHorde.Entities.Generated;

namespace SkytearHorde.Entities.Models.ViewModels
{
    public class CollectionSetDetailRow
    {
        public CollectionCardItemViewModel Item { get; set; }
        public bool AllowCardCollecting { get; set; }
        public VariantTypeViewModel[] VariantTypes { get; set; }

        public CollectionSetDetailRow(CollectionCardItemViewModel item)
        {
            Item = item;

            VariantTypes = [];
        }
    }
}
