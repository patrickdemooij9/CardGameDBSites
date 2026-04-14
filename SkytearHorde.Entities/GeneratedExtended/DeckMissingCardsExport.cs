using SkytearHorde.Business.Exports;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Generated
{
    public partial class DeckMissingCardsExport : IDeckExportType
    {
        public string IconName => "piggy-bank";

        public bool IsCopyToClipboard => false;

        public string GetFileName(Deck deck)
        {
            throw new NotImplementedException();
        }

        public string GetIdentifier()
        {
            throw new NotImplementedException();
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
