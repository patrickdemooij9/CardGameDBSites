using SkytearHorde.Business.Exports;
using SkytearHorde.Entities.Models.Business;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace SkytearHorde.Entities.Generated
{
    public partial class DeckExportGroup : IDeckExportType
    {
        public bool IsCopyToClipboard => false;

        public string IconName => Icon;

        public string GetFileName(Deck deck)
        {
            throw new NotImplementedException();
        }

        public string GetIdentifier()
        {
            return Key.ToString();
        }

        public string GetMimeType()
        {
            throw new NotImplementedException();
        }

        public string GetUrl()
        {
            throw new NotImplementedException();
        }
    }
}
