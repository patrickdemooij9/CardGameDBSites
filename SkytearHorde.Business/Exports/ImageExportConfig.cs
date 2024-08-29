using SkytearHorde.Entities.Generated;

namespace SkytearHorde.Business.Exports
{
    public class ImageExportConfig
    {
        public SortOption[] SortOptions { get; set; }
        public ISquadRequirementConfig[] MainCardLogic { get; set; }
        public bool ShowCardAmounts { get; set; }

        public ImageExportConfig()
        {
            SortOptions = Array.Empty<SortOption>();
            MainCardLogic = Array.Empty<ISquadRequirementConfig>();
            ShowCardAmounts = false;
        }
    }
}
