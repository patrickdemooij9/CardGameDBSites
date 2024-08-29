namespace SkytearHorde.Entities.Models.ViewModels
{
    public class CardOverviewRowDataModel
    {
        public CardItemViewModel Card { get; set; }
        public VariantTypeViewModel[] VariantTypes { get; set; }
        public Dictionary<string, string> AbilitiesToShow { get; set; }
        public bool ShowCollection { get; set; }

        public CardOverviewRowDataModel(CardItemViewModel card)
        {
            Card = card;

            VariantTypes = [];
            AbilitiesToShow = [];
        }
    }
}
