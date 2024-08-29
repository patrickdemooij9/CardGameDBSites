using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Models.Business;
using System.Text;
using Umbraco.Extensions;

namespace SkytearHorde.Business.Exports
{
    public class SPTExport : IDeckExport
    {
        private readonly CardService _cardService;

        public SPTExport(CardService cardService)
        {
            _cardService = cardService;
        }

        public Task<byte[]> ExportDeck(Deck deck)
        {
            var sptCodes = deck.Cards
                .Select(it => _cardService.Get(it.CardId)?.GetMultipleCardAttributeValue("SPT Code")?.FirstOrDefault())
                .WhereNotNull()
                .ToArray();

            return Task.FromResult(Encoding.UTF8.GetBytes($"spt[{string.Join(',', sptCodes)}]"));
        }
    }
}
