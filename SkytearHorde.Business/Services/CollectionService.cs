using Org.BouncyCastle.Asn1.Cms;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.PostModels;
using Umbraco.Extensions;
using Card = SkytearHorde.Entities.Models.Business.Card;

namespace SkytearHorde.Business.Services
{
    public class CollectionService
    {
        private readonly CollectionSetRepository _collectionSetRepository;
        private readonly CollectionCardRepository _collectionCardRepository;
        private readonly CollectionPackRepository _collectionPackRepository;
        private readonly MemberInfoService _memberInfoService;
        private readonly SettingsService _settingsService;
        private readonly CardService _cardService;
        private readonly ISiteService _siteService;

        public CollectionService(CollectionSetRepository collectionSetRepository,
            CollectionCardRepository collectionCardRepository,
            CollectionPackRepository collectionPackRepository,
            MemberInfoService memberInfoService,
            SettingsService settingsService,
            CardService cardService,
            ISiteService siteService)
        {
            _collectionSetRepository = collectionSetRepository;
            _collectionCardRepository = collectionCardRepository;
            _collectionPackRepository = collectionPackRepository;
            _memberInfoService = memberInfoService;
            _settingsService = settingsService;
            _cardService = cardService;
            _siteService = siteService;
        }

        public void AddSetToCollection(int setId, int amount)
        {
            var memberId = AssertLoggedInMember();

            var item = new CollectionSetItem
            {
                SetId = setId,
                Amount = amount,
                UserId = memberId
            };
            _collectionSetRepository.Add(item);

            var cards = _cardService.GetAllBaseBySet(setId);
            var currentCollectionCards = _collectionCardRepository.Get(memberId);
            foreach (var card in cards)
            {
                var cardAmount = 1;
                var cardInCollection = currentCollectionCards.FirstOrDefault(it => it.CardId == card.BaseId && it.VariantId == card.VariantId);
                if (cardInCollection != null)
                {
                    cardAmount += cardInCollection.Amount;
                }
                UpdateCard(card.BaseId, cardAmount, memberId, card.VariantId, cardInCollection);
            }
        }

        public void AddCard(int cardId, int variantId, int amount)
        {
            var memberId = AssertLoggedInMember();

            var currentCards = _collectionCardRepository.Get(cardId, memberId);
            var normalCard = currentCards.FirstOrDefault(it => it.VariantId == variantId);

            UpdateCard(cardId, (normalCard?.Amount ?? 0) + amount, memberId, variantId, normalCard);
        }

        public void UpdateCard(int cardId, int variantId, int amount)
        {
            var memberId = AssertLoggedInMember();

            var collectionCard = _collectionCardRepository.Get(cardId, variantId, memberId);
            UpdateCard(cardId, amount, memberId, variantId, collectionCard);
        }

        public void UpdateCard(int cardId, Dictionary<int, int> variants)
        {
            var memberId = AssertLoggedInMember();

            var currentCards = _collectionCardRepository.Get(cardId, memberId);

            foreach (var keyValue in variants)
            {
                var variantCard = currentCards.FirstOrDefault(it => it.VariantId == keyValue.Key);
                UpdateCard(cardId, keyValue.Value, memberId, keyValue.Key, variantCard);
            }
        }

        public void ClearCollection()
        {
            var memberId = AssertLoggedInMember();

            _collectionCardRepository.Remove(memberId);
        }

        public Card[] ValidateCardsInPack(int setId, PackItemPostModel[] items, out List<PackItemPostModel> invalidItems)
        {
            var cards = _cardService.GetAllBySet(setId, true).Where(it => it.VariantId > 0).ToArray();
            var variants = GetVariantTypes().ToArray();
            var mainIdentifier = _settingsService.GetCollectionSettings().MainIdentifier;

            var cardsToAdd = new List<Card>();
            invalidItems = new List<PackItemPostModel>();
            foreach (var item in items)
            {
                string identifierName = mainIdentifier;

                var card = cards.FirstOrDefault(it => it.GetMultipleCardAttributeValue(identifierName)?.Contains(item.Id) is true && it.VariantTypeId == item.VariantTypeId);
                if (card != null)
                {
                    cardsToAdd.Add(card);
                    continue;
                }

                // Cannot find card, check if variant is child of another variant.
                var selectedVariant = variants.FirstOrDefault(it => it.Id == item.VariantTypeId);
                if (selectedVariant is null || (!selectedVariant.ChildOfBase && selectedVariant.ChildOf is null))
                {
                    // Variant doesn't have a parent, so it is invalid
                    invalidItems.Add(item);
                    continue;
                };

                var parentCard = cards.FirstOrDefault(it => it.GetMultipleCardAttributeValue(identifierName)?.Contains(item.Id) is true && it.VariantTypeId == selectedVariant.ChildOf);
                if (parentCard is null)
                {
                    // Can't find parent so must be invalid
                    invalidItems.Add(item);
                    continue;
                }

                // Found parent card, so let's find the child with the requested variant type
                card = cards.FirstOrDefault(it => it.BaseId == parentCard.BaseId && it.VariantTypeId == item.VariantTypeId);
                if (card is null)
                {
                    invalidItems.Add(item);
                    continue;
                }

                cardsToAdd.Add(card);
            }

            return cardsToAdd.ToArray();
        }

        public void AddPack(int setId, PackItemPostModel[] items)
        {
            var memberId = AssertLoggedInMember();

            var set = _cardService.GetAllSets().FirstOrDefault(it => it.Id == setId);
            if (set is null) return;

            var allCards = _cardService.GetAllBySet(set.Id).ToArray();
            var variants = GetVariantTypes().ToArray();
            foreach (var item in items)
            {
                var card = allCards.FirstOrDefault(it => it.BaseId == int.Parse(item.Id));
                if (card is null) continue;

                var cardVariants = GetVariants(card, set.Id);
                var variant = cardVariants.FirstOrDefault(it => it.VariantTypeId == item.VariantTypeId);
                if (variant is null) continue;

                var currentItem = _collectionCardRepository.Get(card.BaseId, variant.VariantId, memberId);
                UpdateCard(card.BaseId, (currentItem?.Amount ?? 0) + 1, memberId, variant.VariantId, currentItem);
            }

            // Content if we ever want to do something with it. If we do, I'll make sure to deserialize this format and put it in correctly.
            var content = items.Select(it => $"{it.Id}-{it.VariantTypeId}");
            _collectionPackRepository.AddPack(set.Id, string.Join(',', content), memberId);
        }

        private void UpdateCard(int cardId, int newAmount, int memberId, int variantId, CollectionCardItem? existingCard)
        {
            if (existingCard is null && newAmount <= 0) return;

            // Added new card
            if (existingCard is null && newAmount > 0)
            {
                _collectionCardRepository.Add(new CollectionCardItem
                {
                    CardId = cardId,
                    UserId = memberId,
                    VariantId = variantId,
                    Amount = newAmount
                });
                return;
            }

            // No change
            if (newAmount == existingCard!.Amount) return;

            if (newAmount == 0)
            {
                _collectionCardRepository.Remove(existingCard);
                return;
            }
            existingCard.Amount = newAmount;
            _collectionCardRepository.Update(existingCard);
        }

        public void RemoveSetFromCollection(int setId)
        {
            var memberId = AssertLoggedInMember();

            var item = _collectionSetRepository.Get(setId, memberId);
            if (item != null)
            {
                _collectionSetRepository.Remove(item.Id);
            }

            var setCards = _cardService.GetAllBaseBySet(setId);
            var collectionCards = _collectionCardRepository.Get(memberId);
            foreach (var card in setCards)
            {
                var newAmount = 0;
                var cardInCollection = collectionCards.FirstOrDefault(it => it.CardId == card.BaseId && it.VariantId == card.VariantId);
                if (cardInCollection != null)
                {
                    newAmount = cardInCollection.Amount - 1;
                }

                UpdateCard(card.BaseId, newAmount, memberId, card.VariantId, cardInCollection);
            }
        }

        public CollectionSetItem[] GetSets()
        {
            var memberId = AssertLoggedInMember();
            return _collectionSetRepository.Get(memberId);
        }

        public CollectionCardItem[] GetCards()
        {
            var memberId = AssertLoggedInMember();
            var settings = _settingsService.GetCollectionSettings();
            if (settings.AllowCardCollecting)
            {
                return _collectionCardRepository.Get(memberId);
            }
            else if (settings.AllowSetCollecting)
            {
                var sets = _collectionSetRepository.Get(memberId);
                var cards = new List<CollectionCardItem>();

                //TODO: Introduce function to get all the cards for the listed sets
                foreach (var set in sets)
                {
                    cards.AddRange(_cardService.GetAllBySet(set.SetId, false).Select(it => new CollectionCardItem
                    {
                        CardId = it.BaseId,
                        VariantId = 0,
                        Amount = 1,
                        UserId = memberId
                    }));
                }
                return cards.ToArray();
            }
            return Array.Empty<CollectionCardItem>();
        }

        public double CalculateDeckCollection(Deck deck)
        {
            var member = AssertLoggedInMember();
            var cards = GetCards().GroupBy(it => it.CardId).ToDictionary(it => it.Key, it => it);

            var ownedCards = 0;
            foreach (var card in deck.Cards)
            {
                if (!cards.TryGetValue(card.CardId, out var collectionCards))
                {
                    continue;
                }

                ownedCards += Math.Min(card.Amount, collectionCards.Sum(it => it.Amount));
            }
            return ((double)ownedCards / deck.Cards.Sum(it => it.Amount)) * 100;
        }

        public IEnumerable<VariantType> GetVariantTypes()
        {
            return _siteService.GetRoot()
                .FirstChild<Data>()?
                .FirstChild<VariantsContainer>()?
                .Children<Variant>()?
                .Select(it => new VariantType(it)) ?? [];
        }

        public IEnumerable<Card> GetVariants(Card card, int setId)
        {
            return _cardService.GetVariants(card.BaseId)
                .Where(it => it.SetId == setId)
                .OrderBy(it => it.VariantTypeId)
                .ToArray();
            /*var allVariants = GetVariantTypes().ToArray();
            var requireFetchingPages = allVariants.Any(it => it.RequiresPage);

            var variantPages = requireFetchingPages ? _cardService.GetVariants(card.BaseId)
                .Where(it => it.SetId == setId)
                .ToArray() : Array.Empty<Card>();

            foreach (var variant in allVariants)
            {
                if (variant.RequiresPage)
                {
                    if (variantPages.Any(it => it.VariantTypeId == variant.Id))
                        yield return variant;

                    continue;
                }

                var requirements = variant.Requirements;
                if (requirements.Length == 0)
                {
                    yield return variant;
                    continue;
                }
                if (card.MatchesRequirements(requirements))
                {
                    yield return variant;
                }
            }*/
        }

        public int GetPackCount()
        {
            var memberId = AssertLoggedInMember();

            return _collectionPackRepository.GetCount(memberId);
        }

        public int CalculateCollectionProgress()
        {
            var settings = _settingsService.GetCollectionSettings();
            if (settings.AllowCardCollecting)
                return CalculateProgressBasedOnCards();
            if (settings.AllowSetCollecting)
                return CalculateProgressBasedOnSets();
            return 0;
        }

        public bool ShowProgressBar()
        {
            return _settingsService.GetCollectionSettings().ShowProgressBar;
        }

        public int CalculateCollectionProgressBySet(int setId, out int totalCards, out int collectionCards)
        {
            var allCards = _cardService.GetAllBySet(setId).ToArray();
            var allCardsInCollection = GetCards().Select(it => it.CardId).ToArray();

            totalCards = allCards.Length;
            collectionCards = (allCards.Where(it => allCardsInCollection.Contains(it.BaseId)).Count());

            return (int)(collectionCards / (double)allCards.Length * 100);
        }

        private int CalculateProgressBasedOnSets()
        {
            var allSets = _cardService.GetAllSets().ToArray();
            var allSetsInCollection = GetSets().Select(it => it.SetId).ToArray();

            return (int)(allSets.Where(it => allSetsInCollection.Contains(it.Id)).Count() / (double)allSets.Length * 100);
        }

        private int CalculateProgressBasedOnCards()
        {
            var allCards = _cardService.GetAll().ToArray();
            var allCardsInCollection = GetCards().Select(it => it.CardId).ToArray();

            return (int)(allCards.Where(it => allCardsInCollection.Contains(it.BaseId)).Count() / (double)allCards.Length * 100);
        }

        private int AssertLoggedInMember()
        {
            var member = _memberInfoService.GetMemberInfo() ?? throw new InvalidOperationException("No logged in user");
            return member.Id;
        }
    }
}
