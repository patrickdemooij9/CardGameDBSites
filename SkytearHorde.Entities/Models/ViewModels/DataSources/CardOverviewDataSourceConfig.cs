using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Models.ViewModels.DataSources
{
    public class CardOverviewDataSourceConfig : OverviewDataSourceConfig
    {
        public Func<Card, bool>? IsValid { get; set; }
        public Dictionary<string, string> AttributesToShow { get; set; }
        public bool ShowCollection { get; set; }
        public int VariantTypeId { get; set; }

        public CardOverviewDataSourceConfig()
        {
            AttributesToShow = [];
        }
    }
}
