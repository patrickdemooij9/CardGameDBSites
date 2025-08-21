using SkytearHorde.Business.Extensions;
using SkytearHorde.Business.Services.Site;
using SkytearHorde.Entities.Generated;
using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using Card = SkytearHorde.Entities.Models.Business.Card;
using CardAttribute = SkytearHorde.Entities.Models.Business.CardAttribute;
using UmbracoCard = SkytearHorde.Entities.Generated.Card;

namespace SkytearHorde.Business.Repositories
{
    public class CardRepository
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ISiteService _siteService;
        private readonly IProfiler _profiler;

        public CardRepository(IUmbracoContextFactory umbracoContextFactory,
            ISiteService siteService,
            IProfiler profiler)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _siteService = siteService;
            _profiler = profiler;
        }

        public Card? Get(int id)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var umbracoItem = ctx.UmbracoContext.Content?.GetById(id);

            return umbracoItem is UmbracoCard umbracoCard ? Map(umbracoCard) : Map(umbracoItem as CardVariant);
        }

        public IEnumerable<Card> Get(int[] ids)
        {
            foreach (var id in ids)
            {
                var card = Get(id);
                if (card != null)
                    yield return card;
            }
        }

        public Card? GetVariant(int id)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var umbracoItem = ctx.UmbracoContext.Content?.GetById(id);

            return umbracoItem is CardVariant cardVariant ? Map(cardVariant) : null;
        }

        public IEnumerable<Card> GetVariantsForVariant(int variantId)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var umbracoItem = ctx.UmbracoContext.Content?.GetById(variantId) as CardVariant;

            return umbracoItem?
                .Parent?
                .Children<CardVariant>()?
                .Where(it => it.Set?.Id == umbracoItem.Set?.Id)
                .Select(it => Map(it))
                .ToArray() ?? Enumerable.Empty<Card>();
        }

        public IEnumerable<Card> GetVariants(int cardId)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var umbracoItem = ctx.UmbracoContext.Content?.GetById(cardId);

            return umbracoItem?
                .Children<CardVariant>()?
                .Select(it => Map(it))
                .ToArray() ?? Enumerable.Empty<Card>();
        }

        public IEnumerable<Card> GetBase(int cardId)
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();
            var umbracoItem = ctx.UmbracoContext.Content?.GetById(cardId);

            return umbracoItem?
                .Children<CardVariant>()?
                .Where(it => it.VariantType is null)
                .Select(it => Map(it))
                .ToArray() ?? Enumerable.Empty<Card>();
        }

        public IEnumerable<Card> GetAll(bool includeVariants = false)
        {
            using (_profiler.Step("GetAll"))
            {
                using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

                var data = _siteService.GetRoot()
                    .FirstChild<Data>();
                if (data is null) return [];

                var cardContainer = data.FirstChild<CardContainer>();
                if (cardContainer != null)
                {
                    return cardContainer.Children<UmbracoCard>()?
                        .SelectMany(it => MapMultiple(it, includeVariants))
                        .ToArray() ?? [];
                }
                else
                {
                    return data
                        .FirstChild<SetContainer>()?
                        .Children()
                        .SelectMany(it => it.Children<UmbracoCard>() ?? Enumerable.Empty<UmbracoCard>())
                        .SelectMany(it => MapMultiple(it, includeVariants))
                        .ToArray() ?? [];
                }
            }
        }

        public IEnumerable<Card> GetAllBySetCode(string setCode, bool includeVariants = false)
        {
            using (_profiler.Step("GetAllBySetCode"))
            {
                using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

                var cardContainer = _siteService.GetRoot()
                    .FirstChild<Data>()?
                    .FirstChild<CardContainer>();
                if (cardContainer != null)
                {
                    var set = GetAllSets()?.FirstOrDefault(it => it.SetCode?.Equals(setCode, StringComparison.InvariantCultureIgnoreCase) is true);
                    if (set is null) return [];

                    return cardContainer.Children<UmbracoCard>()?
                        .Where(it => it.Set?.Any(it => it.Id == set.Id) is true)
                        .SelectMany(it => MapMultiple(it, includeVariants, set.Id))
                        .ToArray() ?? [];
                }

                return GetAllSets()?
                    .Where(it => it.SetCode?.Equals(setCode, StringComparison.InvariantCultureIgnoreCase) is true)
                    .SelectMany(it => it.Children<UmbracoCard>() ?? Enumerable.Empty<UmbracoCard>())?
                    .SelectMany(it => MapMultiple(it, includeVariants))
                    .ToArray() ?? Array.Empty<Card>();
            }
        }

        public IEnumerable<Set> GetAllSets()
        {
            using var ctx = _umbracoContextFactory.EnsureUmbracoContext();

            return _siteService.GetRoot().FirstChild<Data>()?.FirstChild<SetContainer>()?.Children<Set>() ?? Enumerable.Empty<Set>();
        }

        private IEnumerable<Card> MapMultiple(UmbracoCard card, bool withVariants, int? setId = null)
        {
            yield return Map(card);
            if (!withVariants)
                yield break;

            var children = card.Children<CardVariant>();
            if (setId.HasValue)
            {
                children = children?.Where(it => it.Set?.Id == setId);
            }
            foreach (var variant in children ?? [])
            {
                yield return Map(variant);
            }
        }

        [return: NotNullIfNotNull(nameof(card))]
        private Card? Map(UmbracoCard? card)
        {
            if (card is null) return null;

            var set = card.Set?.OfType<Set>().FirstOrDefault();
            return new Card(card.Id)
            {
                DisplayName = card.DisplayName!,
                SetId = set?.Id ?? card.Parent!.Id,
                SetName = set?.Name ?? card.Parent!.Name,
                UrlSegment = card.UrlSegment!,
                Image = card.Image,
                BackImage = card.BackImage,
                HideFromDecks = card.HideFromDecks,
                EmbedFooterText = card.EmbedFooterText,
                CreatedDate = card.CreateDate,
                Attributes = card.Attributes?.OfType<BlockListItem>()
                    .Select(it => it.Content as IAbilityValue)
                    .WhereNotNull()
                    .ToDictionary(it => it.GetAbility().Name, it => it) ?? new Dictionary<string, IAbilityValue>(),
                Questions = card.Questions.ToItems<FrequentlyAskedQuestion>().ToArray(),
                FaqLink = card.FaqLink,
                Sections = card.Sections.ToItems<CardSection>().ToArray(),
                SlotTargetRequirements = card.SlotTargetRequirements.ToItems<SlotTargetRequirement>().ToArray(),
                SquadRequirements = card.SquadRequirements.ToItems<ISquadRequirementConfig>().ToArray(),
                TeamRequirements = card.TeamRequirements.ToItems<ISquadRequirementConfig>().ToArray(),
                AllowedChildren = card.AllowedChildren?.Select(it => it.Id).ToArray() ?? [],
                MaxChildren = card.MaxChildren,
                Mutations = card.DeckMutations?.ToItems<DeckMutation>().Select(it => new Entities.Models.Business.DeckMutation
                {
                    Alias = it.Alias!,
                    Change = it.Change,
                    SlotId = it.SlotId
                }).ToArray() ?? [],
                NonLegalDeckTypes = card.NonLegalDeckTypes?.OfType<SquadSettings>().Select(it => it.TypeID).ToArray() ?? []
            };
        }

        [return: NotNullIfNotNull(nameof(variant))]
        private Card? Map(CardVariant? variant)
        {
            if (variant is null) return null;
            var umbracoCard = variant.Parent<UmbracoCard>()!;

            var cardAttributes = umbracoCard.Attributes?.OfType<BlockListItem>()
                    .Select(it => it.Content as IAbilityValue)
                    .WhereNotNull()
                    .ToList() ?? new List<IAbilityValue>();

            if (variant.VariantType != null)
            {
                cardAttributes = cardAttributes.Where(it => it.Ability?.Name != "TcgPlayerId").ToList(); //TODO: Fix
            }

            Card card;
            var variantType = variant.VariantType as Variant;
            if (variantType?.ChildOf is Variant || variantType?.ChildOfBase is true)
            {
                var childOfVariant = variantType?.ChildOf as Variant;
                var childOfCard = variant.Siblings<CardVariant>()?.FirstOrDefault(it =>
                {
                    if (variantType?.ChildOfBase is true)
                    {
                        return it.Set?.Id == variant.Set?.Id && it.VariantType?.Id == null;
                    }
                    return it.Set?.Id == variant.Set?.Id && it.VariantType?.Id == childOfVariant?.Id;
                });

                card = childOfCard is null ? Map(umbracoCard)! : Map(childOfCard);

                if (childOfCard != null)
                {
                    foreach (var variantAttribute in childOfCard.Attributes?.OfType<BlockListItem>() ?? Enumerable.Empty<BlockListItem>())
                    {
                        if (variantAttribute.Content is not IAbilityValue attribute) continue;

                        var existingAttribute = cardAttributes.FirstOrDefault(it => it.GetAbility().Key == attribute.Ability!.Key);
                        if (existingAttribute != null)
                        {
                            cardAttributes.Remove(existingAttribute);
                        }
                        cardAttributes.Add(attribute);
                    }
                }
            }
            else
            {
                card = Map(umbracoCard)!;
            }

            foreach (var variantAttribute in variant.Attributes?.OfType<BlockListItem>() ?? Enumerable.Empty<BlockListItem>())
            {
                if (variantAttribute.Content is not IAbilityValue attribute) continue;

                var existingAttribute = cardAttributes.FirstOrDefault(it => it.GetAbility().Key == attribute.Ability!.Key);
                if (existingAttribute != null)
                {
                    cardAttributes.Remove(existingAttribute);
                }
                cardAttributes.Add(attribute);
            }

            var urlSegment = card.UrlSegment;
            if (variantType != null)
            {
                urlSegment += $"/{variant.UrlSegment}";
            }
            return new Card(card.BaseId)
            {
                VariantId = variant.Id,
                VariantTypeId = variantType?.InternalID,
                DisplayName = variant.DisplayName.IfNullOrWhiteSpace(card.DisplayName),
                SetId = variant.Set?.Id ?? card.SetId,
                SetName = variant.Set?.Name ?? card.SetName,
                UrlSegment = urlSegment,
                Image = variant.Image ?? card.Image,
                BackImage = variant.BackImage ?? card.BackImage,
                HideFromDecks = card.HideFromDecks,
                EmbedFooterText = card.EmbedFooterText,
                CreatedDate = card.CreatedDate,
                Attributes = cardAttributes
                    .ToDictionary(it => it.GetAbility().Name, it => it) ?? new Dictionary<string, IAbilityValue>(),
                Questions = card.Questions,
                FaqLink = card.FaqLink,
                Sections = card.Sections,
                SlotTargetRequirements = card.SlotTargetRequirements,
                SquadRequirements = card.SquadRequirements,
                TeamRequirements = card.TeamRequirements,
                AllowedChildren = card.AllowedChildren,
                MaxChildren = card.MaxChildren,
                Mutations = card.Mutations,
                NonLegalDeckTypes = card.NonLegalDeckTypes
            };
        }
    }
}
