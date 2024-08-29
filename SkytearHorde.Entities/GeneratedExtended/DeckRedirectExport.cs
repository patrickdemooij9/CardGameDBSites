using SkytearHorde.Business.Exports;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Generated
{
    public partial class DeckRedirectExport : IDeckExportType
    {
        public string IconName => Icon;

        public bool IsCopyToClipboard => false;

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
            return "redirect";
        }

        public string GetUrl()
        {
            throw new NotImplementedException();
        }
    }
}
