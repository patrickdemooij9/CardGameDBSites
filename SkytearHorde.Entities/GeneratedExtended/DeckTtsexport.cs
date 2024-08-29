using SkytearHorde.Business.Exports;
using SkytearHorde.Entities.Models.Business;

namespace SkytearHorde.Entities.Generated
{
    public partial class DeckTtsexport : IDeckExportType
    {
        public bool IsCopyToClipboard => false;

        public string IconName => "puzzle-piece";

        public string GetFileName(Deck deck)
        {
            return $"{deck.Name}-TTS.json";
        }

        public string GetIdentifier()
        {
            return this.Key.ToString();
        }

        public string GetMimeType()
        {
            return "application/json";
        }

        public string GetUrl()
        {
            return "/umbraco/api/export/export";
        }
    }
}
