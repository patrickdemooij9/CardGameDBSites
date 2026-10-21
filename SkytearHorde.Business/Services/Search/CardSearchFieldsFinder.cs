using SkytearHorde.Business.Services.Site;
using Umbraco.Cms.Core.Web;

namespace SkytearHorde.Business.Services.Search
{
    public class CardSearchFieldsFinder
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISiteService _siteService;

        public CardSearchFieldsFinder(IUmbracoContextFactory umbracoContextFactory, ISiteService siteService)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _siteService = siteService;
        }

        public string[] GetGeneralFieldsToSearch()
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

            return [.. _siteService.GetAllAttributes().Where(it => it.GeneralSearchable).Select(it => GetTextFieldName(it.Name!))];
        }

        public static string GetTextFieldName(string attributeName) => $"CustomField.{attributeName}.Text";
    }
}
