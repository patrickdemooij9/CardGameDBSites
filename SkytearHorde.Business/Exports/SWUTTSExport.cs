using SkytearHorde.Business.Services;
using SkytearHorde.Business.Utils;
using SkytearHorde.Entities.Models.Business;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkytearHorde.Business.Exports
{
    public class SWUTTSExport : IDeckExport
    {
        private readonly CardService _cardService;

        public SWUTTSExport(CardService cardService)
        {
            _cardService = cardService;
        }

        public Task<byte[]> ExportDeck(Deck deck)
        {
            return Task.FromResult(JsonSerializer.SerializeToUtf8Bytes(DeckJsonFileHelper.ToJsonModel(deck, _cardService)));
        }
    }
}
