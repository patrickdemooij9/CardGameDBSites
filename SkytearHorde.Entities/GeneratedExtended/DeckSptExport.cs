using SkytearHorde.Business.Exports;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Generated
{
    public partial class DeckSptexport : IDeckExportType
    {
        public bool IsCopyToClipboard => true;

        public string IconName => "medal";

        public string GetFileName(Deck deck)
        {
            return "";
        }

        public string GetIdentifier()
        {
            return Key.ToString();
        }

        public string GetMimeType()
        {
            return "text/plain";
        }

        public string GetUrl()
        {
            throw new NotImplementedException();
        }
    }
}
