namespace SkytearHorde.Entities.Models.ViewModels
{
    public class CollectionSetDetailViewModel
    {
        public CollectionCardItemViewModel[] Cards { get; set; }
        public bool AllowCardCollecting { get; set; }
        public VariantTypeViewModel[] VariantTypes { get; set; }

        public CollectionSetDetailViewModel()
        {
            Cards = [];
            VariantTypes = [];
        }
    }
}
