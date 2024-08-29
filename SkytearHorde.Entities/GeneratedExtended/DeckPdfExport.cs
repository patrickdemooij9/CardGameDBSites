using SkytearHorde.Business.Exports;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Generated
{
    public partial class DeckPdfExport : IDeckExportType
    {
        public bool IsCopyToClipboard => false;

        public string IconName => "cards-three";

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
            return "application/pdf";
        }

        public string GetUrl()
        {
            return "/umbraco/api/export/pdfexport";
        }
    }
}
