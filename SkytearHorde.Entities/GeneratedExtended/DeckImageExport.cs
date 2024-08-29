using SkytearHorde.Business.Exports;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Generated
{
    public partial class DeckImageExport : IDeckExportType
    {
        public bool IsCopyToClipboard => false;

        public string IconName => "image";

        public string GetFileName(Deck deck)
        {
            return $"{deck.Name}.png";
        }

        public string GetIdentifier()
        {
            return Key.ToString();
        }

        public string GetMimeType()
        {
            return "image/png";
        }

        public string GetUrl()
        {
            throw new NotImplementedException();
        }
    }
}
