using Umbraco.Cms.Core.Models;

namespace SkytearHorde.Entities.Models.ViewModels
{
    public class CardOverviewDataModel
    {
        public Dictionary<string, string> AbilitiesToShow { get; set; }
        public PagedResult<CardItemViewModel> Cards { get; set; }
        public VariantTypeViewModel[] VariantTypes { get; set; }
        public bool ShowCollection { get; set; }
        public int Page { get; set; }
        public string BaseUrl { get; set; }

        public CardOverviewDataModel()
        {
            AbilitiesToShow = [];
            Cards = new PagedResult<CardItemViewModel>(0, 1, 0);
            VariantTypes = [];
        }
    }
}
