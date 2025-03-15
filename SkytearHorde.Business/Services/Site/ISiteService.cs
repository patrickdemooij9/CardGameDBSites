using SkytearHorde.Entities.Generated;

namespace SkytearHorde.Business.Services.Site
{
    public interface ISiteService
    {
        Homepage GetRoot();
        Settings GetSettings();

        CardOverview GetCardOverview();
        DeckOverview GetDeckOverview(int typeId);
        DeckOverview[] GetDeckOverviews();
        SetOverview? GetSetOverview();
        CollectionPage? GetCollectionPage();

        int[] GetAllSites();

        CardAttribute[] GetAllAttributes();
    }
}
