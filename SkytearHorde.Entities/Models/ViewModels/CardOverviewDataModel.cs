namespace SkytearHorde.Entities.Models.ViewModels
{
    public class CardOverviewDataModel
    {
        public Dictionary<string, string> AbilitiesToShow { get; set; }
        public CardItemViewModel[] Cards { get; set; }
        public VariantTypeViewModel[] VariantTypes { get; set; }
        public bool ShowCollection { get; set; }

        public CardOverviewDataModel()
        {
            AbilitiesToShow = new Dictionary<string, string>();
            Cards = Array.Empty<CardItemViewModel>();
        }
    }
}
