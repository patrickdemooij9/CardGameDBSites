using SkytearHorde.Business.Services;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.TTS;
using System.Text.Json;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using Card = SkytearHorde.Entities.Models.TTS.Card;

namespace SkytearHorde.Business.Exports
{
    public class TTSExport : IDeckExport
    {
        private readonly CardService _cardService;
        private readonly TTSExportConfig _config;

        public TTSExport(CardService cardService, TTSExportConfig config)
        {
            _cardService = cardService;
            _config = config;
        }

        public Task<byte[]> ExportDeck(Deck deck)
        {
            var groupedGroups = deck.Cards.GroupBy(it => it.GroupId);
            var model = new RootObject
            {
                ObjectStates = new BaseObjectDefinition[]
                {
                    new BagDefinition
                    {
                        Nickname = deck.Name,
                        ContainedObjects = groupedGroups.SelectMany(it => it.GroupBy(g => g.SlotId).Select(g => GetDeck(g.ToArray()))).ToArray(),
                        Transform = GetDefaultTransform(false)
                    }
                }
            };

            return Task.FromResult(JsonSerializer.SerializeToUtf8Bytes(model));
        }

        private Transform GetDefaultTransform(bool upsideDown)
        {
            return new Transform
            {
                rotY = upsideDown ? 180 : 0,
                rotZ = upsideDown ? 180 : 0,
                scaleX = _config.Width ?? 1f,
                scaleY = 1,
                scaleZ = _config.Height ?? 1f
            };
        }

        private BaseObjectDefinition GetDeck(DeckCard[] cards)
        {
            if (cards.Length == 1 && cards.Sum(it => it.Amount) == 1)
            {
                return GetCard(_cardService.Get(cards[0].CardId)!);
            }

            if (_config.OrderDescending) cards = cards.Reverse().ToArray();

            var customDeck = new Dictionary<string, Card>();
            var cardDefinition = new List<CardDefinition>();
            var cardIds = new List<int>();
            var number = 1;
            foreach (var deckCard in cards)
            {
                var card = _cardService.Get(deckCard.CardId)!;
                var groupedNumber = int.Parse($"{number}{(number - 1).ToString("00")}");

                for (var i = 0; i < deckCard.Amount; i++)
                {
                    customDeck.Add(number.ToString(), new Card
                    {
                        FaceURL = GetImage(card.Image!),
                        BackURL = card.BackImage != null ? GetImage(card.BackImage) : _config.BackImageUrl,
                        NumHeight = 1,
                        NumWidth = 1,
                        BackIsHidden = true
                    });

                    cardDefinition.Add(new CardDefinition
                    {
                        CardId = groupedNumber,
                        Nickname = card.DisplayName,
                        Transform = GetDefaultTransform(true)
                    });

                    cardIds.Add(int.Parse($"{number}{(number - 1).ToString("00")}"));

                    number++;
                }
            }

            return new CustomDeckDefinition
            {
                ContainedObjects = cardDefinition.ToArray(),
                DeckIDs = cardIds.ToArray(),
                CustomDeck = customDeck,
                Transform = GetDefaultTransform(true)
            };
        }

        private CardDefinition GetCard(Entities.Models.Business.Card card)
        {
            return new CardDefinition
            {
                Nickname = card.DisplayName,
                CardId = 100,
                CustomDeck = new Dictionary<string, Card>
                {
                    { "1", new Card
                    {
                        FaceURL = GetImage(card.Image!),
                        BackURL = card.BackImage != null ? GetImage(card.BackImage) : _config.BackImageUrl,
                        NumHeight = 1,
                        NumWidth = 1,
                        BackIsHidden = true
                    } }
                },
                Transform = GetDefaultTransform(true)
            };
        }

        private string GetImage(MediaWithCrops image)
        {
            var ttsImage = image.Value<MediaWithCrops>("ttsImage");
            if (ttsImage != null)
            {
                return ttsImage.Url(mode: UrlMode.Absolute);
            }
            return image.Url(mode: UrlMode.Absolute);
        }
    }
}
