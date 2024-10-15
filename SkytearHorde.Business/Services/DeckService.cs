using SixLabors.ImageSharp;
using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Helpers;
using SkytearHorde.Business.Middleware;
using SkytearHorde.Business.Repositories;
using SkytearHorde.Entities.Enums;
using SkytearHorde.Entities.Generated;
using SkytearHorde.Entities.Models.Business;
using SkytearHorde.Entities.Models.Business.Repository;
using SkytearHorde.Entities.Models.PostModels;
using SkytearHorde.Entities.Models.ViewModels;
using System.Diagnostics;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;
using Card = SkytearHorde.Entities.Models.Business.Card;

namespace SkytearHorde.Business.Services
{
    public class DeckService
    {
        private readonly SettingsService _settingsService;
        private readonly CardService _cardService;
        private readonly DeckRepository _deckRepository;
        private readonly IAbilityFormatter _abilityFormatter;
        private readonly ISiteAccessor _siteAccessor;
        private readonly DeckLikeRepository _deckLikeRepository;
        private readonly DeckCalculateScoreRepository _deckCalculateScoreRepository;
        private readonly DeckViewRepository _deckViewRepository;
        private readonly IProfiler _profiler;
        private readonly IAppPolicyCache _cache;

        public DeckService(SettingsService settingsService, CardService cardService, DeckRepository deckRepository,
            IAbilityFormatter abilityFormatter, ISiteAccessor siteAccessor, DeckLikeRepository deckLikeRepository, DeckCalculateScoreRepository deckCalculateScoreRepository, DeckViewRepository deckViewRepository, AppCaches caches, IProfiler profiler)
        {
            _settingsService = settingsService;
            _cardService = cardService;
            _deckRepository = deckRepository;
            _abilityFormatter = abilityFormatter;
            _siteAccessor = siteAccessor;
            _deckLikeRepository = deckLikeRepository;
            _deckCalculateScoreRepository = deckCalculateScoreRepository;
            _deckViewRepository = deckViewRepository;
            _profiler = profiler;
            _cache = caches.RuntimeCache;
        }

        public int ProcessDeck(CreateSquadPostModel postModel, bool publish, int? userId)
        {
            var createdDate = DateTime.UtcNow;

            // If existing deck, check if user has access.
            if (postModel.Id.HasValue && postModel.Id != 0)
            {
                var existingDeck = Get(postModel.Id.Value, DeckStatus.None);
                if (existingDeck is null || existingDeck.CreatedBy is null || existingDeck.CreatedBy != userId) throw new InvalidOperationException("User should have no access to this deck");

                createdDate = existingDeck.CreatedDate;
            }

            var allCards = _cardService.GetAll().Where(it => !it.HideFromDecks).ToDictionary(it => it.BaseId, it => it);
            var squadSettings = _settingsService.GetSquadSettings(postModel.TypeId);

            if (postModel.Squads.Length != squadSettings.Squads.Count) throw new InvalidOperationException("Give squad size is not the same as expected size");

            var allPostedCharacterIds = postModel.Squads.SelectMany(it => it.Slots.SelectMany(s => s.Cards.Select(c => c.CardId))).ToArray();

            if (allPostedCharacterIds.Any(c => !allCards.ContainsKey(c))) throw new InvalidOperationException("Could not find card.");

            var teamCharacters = allPostedCharacterIds.Select(it => allCards[it]).ToArray();
            var teamRequirements = squadSettings.Restrictions.ToItems<ISquadRequirementConfig>().ToList();
            teamRequirements.AddRange(teamCharacters?.SelectMany(it => it.TeamRequirements) ?? Enumerable.Empty<ISquadRequirementConfig>());

            foreach (var teamRequirement in teamRequirements)
            {
                if (!teamRequirement.GetRequirement().IsValid(teamCharacters))
                    throw new InvalidOperationException("Not valid");
            }

            foreach (var squadConfig in squadSettings.Squads.ToItems<SquadConfig>())
            {
                var postSquad = postModel.Squads.FirstOrDefault(it => it.Id == squadConfig.SquadId);
                if (postSquad is null) throw new InvalidOperationException($"Could not find squad with given id: {squadConfig.SquadId}");

                var postCharacters = postSquad.Slots.SelectMany(it => it.Cards.Select(c => c.CardId)).ToArray();

                // Check if all selected cards in the squad are valid
                if (postCharacters.Any(it => !allCards.ContainsKey(it))) throw new InvalidOperationException("Could not find card.");

                var characters = postCharacters.Where(it => it != null).Select(it => allCards[it]).ToArray();

                var squadRequirements = squadConfig.Requirements.ToItems<ISquadRequirementConfig>().ToList();
                squadRequirements.AddRange(characters?.SelectMany(it => it.SquadRequirements) ?? Enumerable.Empty<ISquadRequirementConfig>());
                foreach (var requirement in squadRequirements)
                {
                    if (!requirement.GetRequirement().IsValid(characters))
                        throw new InvalidOperationException("Not valid");
                }

                var overwriteAmount = squadSettings.OverwriteAmount > 0;
                var additionalSquadRequirements = new Dictionary<int, List<ISquadRequirementConfig>>();

                var squadSlotConfigs = squadConfig.Slots.ToItems<SquadSlotConfig>().ToArray();
                for (var i = 0; i < squadSlotConfigs.Length; i++)
                {
                    var postedSlot = postSquad.Slots.FirstOrDefault(it => it.Id == i);
                    if (postedSlot is null) throw new InvalidOperationException("Slot not found in post model");

                    var slotConfig = squadSlotConfigs[i];

                    if (publish)
                    {
                        if (slotConfig.MaxCards > 0 && slotConfig.MaxCards != postedSlot.Cards.Sum(it => it.Amount)) throw new InvalidOperationException("Not max cards given");
                        if (slotConfig.MinCards > 0 && slotConfig.MinCards > postedSlot.Cards.Sum(it => it.Amount)) throw new InvalidOperationException("Not min cards given");
                        if (postedSlot.Cards.Any(it => it.Amount > (overwriteAmount ? squadSettings.OverwriteAmount : int.Parse(allCards[it.CardId].GetMultipleCardAttributeValue("Amount")?.First() ?? "1")))) throw new InvalidOperationException("Too many cards of one type");
                    }

                    var cards = postedSlot.Cards.Select(it => allCards[it.CardId]).ToArray();
                    foreach (var additionalRequirement in cards.SelectMany(it => it.SlotTargetRequirements))
                    {
                        var key = additionalRequirement.TargetSlotId;
                        if (additionalSquadRequirements.ContainsKey(key))
                        {
                            additionalSquadRequirements[key].AddRange(additionalRequirement.Requirements.ToItems<ISquadRequirementConfig>());
                        }
                        else
                        {
                            additionalSquadRequirements.Add(key, additionalRequirement.Requirements.ToItems<ISquadRequirementConfig>().ToList());
                        }
                    }

                    var slotRequirements = slotConfig.Requirements.ToItems<ISquadRequirementConfig>().ToList();
                    if (additionalSquadRequirements.ContainsKey(i))
                    {
                        slotRequirements.AddRange(additionalSquadRequirements[i]);
                    }
                    foreach (var requirement in slotRequirements)
                    {
                        if (!requirement.GetRequirement().IsValid(cards))
                            throw new InvalidOperationException("Slot not valid");
                    }
                }
            }

            var deck = new Deck(postModel.Id ?? 0, postModel.Name)
            {
                Description = postModel.Description,
                CreatedBy = userId,
                Cards = postModel.Squads.SelectMany((squad) => squad.Slots.SelectMany((c, index) =>
                {
                    return c.Cards.Select(it => new DeckCard(it.CardId, squad.Id, index, it.Amount));
                })).WhereNotNull().ToList(),
                CreatedDate = createdDate,
                IsPublished = publish,
                SiteId = _siteAccessor.GetSiteId(),
                TypeId = postModel.TypeId
            };
            var deckCalculator = new DeckCalculator();
            int deckId;
            if (!postModel.Id.HasValue || postModel.Id == 0)
            {
                deck.Score = deckCalculator.CalculateDeckScore(deck, Array.Empty<int>());
                deckId = _deckRepository.Create(deck);
            }
            else
            {
                deck.Score = deckCalculator.CalculateDeckScore(deck, _deckViewRepository.GetLast7Days(deck.Id));

                _deckRepository.Update(deck);
                deckId = deck.Id;
            }

            _deckCalculateScoreRepository.ScheduleDeckCalculate(deckId, DateTime.UtcNow.AddDays(1));
            return deckId;
        }

        public Deck? Get(int id, DeckStatus status = DeckStatus.Published)
        {
            var decks = _deckRepository.Get(status, new int[] { id }).ToArray();
            if (decks.Length <= 1) return decks.FirstOrDefault();
            return decks.OrderByDescending(it => it.UpdatedDate).FirstOrDefault();
        }

        public IEnumerable<Deck> Get(IEnumerable<int> ids, DeckStatus status = DeckStatus.Published)
        {
            return _deckRepository.Get(status, ids.ToArray());
        }

        public bool IsPublished(int id)
        {
            return _deckRepository.Get(DeckStatus.Published, id).Any();
        }

        public void DeleteDeck(Deck deck)
        {
            _deckRepository.DeleteDeck(deck.Id);
        }

        public void UpdateScore(Deck deck, int score)
        {
            _deckRepository.SetScore(deck.Id, score);
        }

        public Dictionary<int, bool> Exists(IEnumerable<int> deckIds)
        {
            return _deckRepository.Exists(deckIds);
        }

        public PagedResult<Deck> GetAll(DeckPagedRequest request)
        {
            using var _ = _profiler.Step("GetAllDecks");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var allDecks = _deckRepository.GetAll(_siteAccessor.GetSiteId(), request, out var totalSize).ToArray();
            stopwatch.Stop();
            return new PagedResult<Deck>(totalSize, request.Page, request.Take)
            {
                Items = allDecks
            };
        }

        public PagedResult<Deck> GetAllByUser(int userId, int page = 1, int size = 20)
        {
            // If we have unpublished and published decks as current, only show the latest
            var allDecks = _deckRepository.GetAll(_siteAccessor.GetSiteId(), userId, DeckStatus.None)
                .GroupBy(it => it.Id)
                .Select(it => it.OrderByDescending(d => d.UpdatedDate).First()).ToArray();
            return new PagedResult<Deck>(allDecks.Length, page, size)
            {
                Items = allDecks.Skip((page - 1) * size).Take(size)
            };
        }

        public bool ToggleLikeDeck(Deck deck, int userId)
        {
            var currentLikes = _deckLikeRepository.GetLikedDecks(userId);
            var addedLike = !currentLikes.Contains(deck.Id);
            if (currentLikes.Contains(deck.Id))
                _deckLikeRepository.DeleteLike(userId, deck.Id);
            else
                _deckLikeRepository.AddLike(userId, deck.Id);

            _deckCalculateScoreRepository.ScheduleDeckCalculate(deck.Id, DateTime.UtcNow);

            _deckRepository.ClearCache(deck.Id);
            _cache.ClearByKey(MemberInfoService.CacheKey);

            return addedLike;
        }

        public IEnumerable<Deck> GetHottestDecks(int typeId, int amount)
        {
            return _deckRepository.GetAll(_siteAccessor.GetSiteId(), new DeckPagedRequest(typeId)
            {
                OrderBy = "popular",
                Page = 1,
                Take = amount,
                Status = DeckStatus.Published,
            }, out _);
        }

        public IEnumerable<Card> GetMainCards(Deck deck)
        {
            return GetMainCards(deck.Cards);
        }

        public Dictionary<Color, int> GetColorsByDeck(Deck deck)
        {
            var colors = new Dictionary<Guid, int>();
            var squadSettings = _settingsService.GetSquadSettings(deck.TypeId);
            var colorLogicItems = squadSettings.ColorLogic.ToItems<CardColorLogic>().ToArray();

            if (colorLogicItems.Length == 0)
            {
                return deck.Cards.Select(it => _cardService.Get(it.CardId)?.GetMultipleCardAttributeValue("Color")?.FirstOrDefault())
                .WhereNotNull()
                .GroupBy(it => it)
                .ToDictionary(it => Color.Parse(it.Key), it => it.Count());
            }

            foreach (var colorOption in colorLogicItems)
            {
                colors.Add(colorOption.Key, 0);
            }

            foreach (var card in deck.Cards.Select(it => _cardService.Get(it.CardId)).WhereNotNull())
            {
                foreach (var option in colorLogicItems)
                {
                    var conditionsMet = option.Requirement.ToItems<ISquadRequirementConfig>().Any(it => it.GetRequirement().IsValid(new[] { card }));
                    if (conditionsMet)
                    {
                        colors[option.Key]++;
                    }
                }
            }

            return colorLogicItems.Where(it => colors[it.Key] > 0).ToDictionary(it => Color.Parse(it.Color!), it => colors[it.Key]);
        }
        
        public IEnumerable<Card> GetMainCards(IEnumerable<DeckCard> cards)
        {
            var squadSettings = _settingsService.GetSquadSettings();
            if (squadSettings is null)
            {
                throw new ApplicationException("No squad settings setup!");
            }

            var mainCardRequirements = squadSettings.MainCard.ToItems<ISquadRequirementConfig>().ToArray();
            return cards.Select(it => _cardService.Get(it.CardId)).WhereNotNull().Where(c => mainCardRequirements.All(r => r.GetRequirement().IsValid(new[] {c} ))).ToArray();
        }

        public IEnumerable<CreateDeckViewModel> GetStartingDecks()
        {
            return Enumerable.Empty<CreateDeckViewModel>(); //TODO: Clean up
        }

        public string[] GetOverviewImageForCard(Card card, SquadSettings settings)
        {
            var images = new List<string>();
            if (settings.SpecialImageRule?.Count > 0)
            {
                var cardArray = new[] { card };
                var conditionsMet = settings.SpecialImageRule.ToItems<CardImage>().Where(it => it.Conditions.ToItems<ISquadRequirementConfig>().All(config => config.GetRequirement().IsValid(cardArray)));

                foreach(var conditionMet in conditionsMet)
                {
                    if (conditionMet.Image != null)
                    {
                        images.Add(conditionMet.Image?.GetCropUrl(width: 50, height: 50));
                    }
                }
            }
            return images.ToArray();
        }
    }
}
