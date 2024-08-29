using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Business.DeckExport;

namespace SkytearHorde.Business.Utils
{
    public static class DeckJsonFileHelper
    {
        public static DeckJsonFile ToJsonModel(Deck deck, CardService cardService)
        {
            var cards = deck.Cards.Select(it => cardService.GetVariants(it.CardId).OrderBy(it => it.CreatedDate).First(it => it.VariantTypeId == null)).ToDictionary(it => it.BaseId, it => it);
            var leaderCard = deck.Cards.First(it => cards[it.CardId].GetMultipleCardAttributeValue("Card Type")?.Contains("Leader") is true);
            var baseCard = deck.Cards.First(it => cards[it.CardId].GetMultipleCardAttributeValue("Card Type")?.Contains("Base") is true);
            var model = new DeckJsonFile
            {
                Metadata = new DeckJsonMetaData
                {
                    Name = deck.Name
                },
                Leader = new DeckJsonCard
                {
                    Id = cards[leaderCard.CardId].GetMultipleCardAttributeValue("TTS Id")[0],
                    Count = leaderCard.Amount
                },
                Base = new DeckJsonCard
                {
                    Id = cards[baseCard.CardId].GetMultipleCardAttributeValue("TTS Id")[0],
                    Count = baseCard.Amount
                },
                Deck = deck.Cards.Where(it => it.CardId != leaderCard.CardId && it.CardId != baseCard.CardId).Select(it => new DeckJsonDeckCard
                {
                    Id = cards[it.CardId].GetMultipleCardAttributeValue("TTS Id")[0],
                    Unit = cards[it.CardId].GetMultipleCardAttributeValue("Card Type")[0],
                    Count = it.Amount
                }).ToArray()
            };

            return model;
        }
    }
}
